﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    using Microsoft.Azure.Cosmos.ChangeProcessor.LoadBalancing;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ChangeProcessor<TPartition, TLease, TContinuation>
        where TPartition : IPartition
        where TLease : ILease<TContinuation>
    {
        private readonly string identifier;
        private readonly IPartitioner<TPartition> partitioner;
        private readonly ILeaseContainer<TPartition, TLease, TContinuation> leaseContainer;
        private readonly IProcessor<TLease, TContinuation> processor;
        private readonly ProcessorOptions options;
        private static readonly int DefaultDegreeOfParallelism = 25;
        private CancellationTokenSource shutdownSource = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, Tuple<TLease, Task>> tasks = new ConcurrentDictionary<string, Tuple<TLease, Task>>();

        public ChangeProcessor(
            string identifier, 
            IPartitioner<TPartition> partitioner,
            ILeaseContainer<TPartition, TLease, TContinuation> leaseContainer, 
            IProcessor<TLease, TContinuation> processor, 
            ProcessorOptions options)
        {
            this.identifier = identifier;
            this.partitioner = partitioner;
            this.leaseContainer = leaseContainer;
            this.processor = processor;
            this.options = options;
        }

        public async Task StartAsync()
        {
            shutdownSource = new CancellationTokenSource();
            await this.InitializeLeasesAsync();
            await this.ProcessAsync();
        }

        public async Task StopAsync()
        {
            this.shutdownSource.Cancel();
            await Task.WhenAll(this.tasks.Values.Select(pair => pair.Item2));
            await this.ReleaseLeases();
            this.tasks.Clear();
        }

        private async Task ReleaseLeases()
        {
            await tasks.Values.ForEachAsync(async pair => {
                pair.Item1.SetOwner("");
                await this.leaseContainer.UpdateLeaseAsync(pair.Item1);
              });
        }

        private async Task ProcessAsync()
        {
            IEnumerable<TLease> leases = await this.leaseContainer.GetAllLeasesAsync();
            IEnumerable<TLease> leasesToTake = new EqualPartitionsBalancingStrategy<TLease, TContinuation>(this.identifier, 0, 4, TimeSpan.FromMinutes(10)).SelectLeasesToTake(leases);
            foreach (TLease lease in leasesToTake) 
            {
                Tuple<bool, TLease> tuple = await this.leaseContainer.TakeLeaseAsync(lease);

                if (!tuple.Item1)
                {
                    Trace.Information("Lease {0} was not taken before processing.", lease.Id());
                    continue;
                }

                if (!this.tasks.TryAdd(tuple.Item2.Id(), new Tuple<TLease, Task>(tuple.Item2, this.ProcessLeaseAsync(lease, this.shutdownSource.Token))))
                {
                    throw new Exception("Initialization of leases failed.");
                }
            }
        }

        private async Task ProcessLeaseAsync(TLease lease, CancellationToken cancellationToken)
        {
            try
            {
                bool leaseFailed = false;
                while (!cancellationToken.IsCancellationRequested && !leaseFailed)
                {
                    TimeSpan delay = this.options.PollDelay;

                    Action<TContinuation> saveCheckpoint = async check =>
                    {
                        lease.SetContinuation(check);
                        lease.SetTimestamp(DateTime.UtcNow);
                        var (updated, newLease) = await this.leaseContainer.UpdateLeaseAsync(lease);
                        if (!updated)
                        {
                            Trace.Information("Lease {0} lost during processing.", lease.Id());
                            leaseFailed = true;
                        }
                    };

                    Task<TContinuation> task = this.processor.ProcessAsync(lease, cancellationToken, d => delay = d, saveCheckpoint);

                    using (CancellationTokenSource timeoutCancellation = new CancellationTokenSource())
                    {
                        if (!ReferenceEquals(await Task.WhenAny(task, Task.Delay(this.options.RequestTimeout, timeoutCancellation.Token)), task))
                        {
                            Task catchExceptionFromTask = task.ContinueWith(t => Trace.Information(
                                "Timed out - change request failed with exception: {0}", t.Exception.InnerException),
                                TaskContinuationOptions.OnlyOnFaulted);
                            throw new Exception("Change request timed out");
                        }
                        else
                        {
                            timeoutCancellation.Cancel();
                        }

                        TContinuation newContinuation = await task;
                        if (newContinuation != null)
                        {
                            saveCheckpoint(newContinuation);
                        }
                    }

                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            } 
            catch (OperationCanceledException)
            {
            }
            finally
            {
                await this.leaseContainer.ReleaseLeaseAsync(lease);
            }
        }

        private async Task InitializeLeasesAsync()
        {
            while (!shutdownSource.IsCancellationRequested)
            {
                bool initialized = await this.leaseContainer.IsInitializedAsync();
                if (initialized)
                {
                    break;
                }

                bool isLockAcquired = await this.leaseContainer.LockAsync();

                try
                {
                    if (!isLockAcquired)
                    {
                        Trace.Information("Another instance is initializing the lease container");
                        await Task.Delay(TimeSpan.FromSeconds(15));
                        continue;
                    }

                    Trace.Information("Initializing the lease container");
                    IEnumerable<TPartition> partitions = await this.partitioner.GetPartitionsAsync();
                    await partitions.ForEachAsync(
                        async partition => await this.leaseContainer.CreateLeaseAsync(partition),
                        DefaultDegreeOfParallelism);
                    await this.leaseContainer.MarkInitializedAsync();
                }
                finally
                {
                    if (isLockAcquired)
                    {
                        await this.leaseContainer.UnlockAsync();
                    }
                }

                break;
            }

            Trace.Information("The lease container is initialized");
        }
    }
}
