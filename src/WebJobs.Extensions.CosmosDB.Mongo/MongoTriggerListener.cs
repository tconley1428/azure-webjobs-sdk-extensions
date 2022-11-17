// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.ChangeProcessor;
using Microsoft.Azure.Cosmos.ChangeProcessor.Mongo;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using MongoDB.Bson;

namespace Microsoft.Azure.WebJobs.Extensions.CosmosDB.Mongo
{
    public class MongoTriggerListener : IListener
    {
        private ChangeProcessor<MongoPartition, MongoLease, BsonDocument> changeProcessor;

        public MongoTriggerListener(ITriggeredFunctionExecutor executor, MongoCollectionReference monitoredCollection, MongoCollectionReference leaseCollection)
        {
            string id = Guid.NewGuid().ToString();

            MongoPartitioner partitioner = new MongoPartitioner(monitoredCollection.Client.GetDatabase(monitoredCollection.DatabaseName), monitoredCollection.CollectionName);
            MongoLeaseContainer leaseContainer = new MongoLeaseContainer(leaseCollection.Client.GetDatabase(leaseCollection.DatabaseName).GetCollection<BsonDocument>(leaseCollection.CollectionName), id);

            MongoProcessor processor = new MongoProcessor(monitoredCollection.Client.GetDatabase(monitoredCollection.DatabaseName).GetCollection<BsonDocument>(monitoredCollection.CollectionName), 
                async doc => await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = doc }, CancellationToken.None));

            this.changeProcessor = new ChangeProcessor<MongoPartition, MongoLease, BsonDocument>(
                id, partitioner, leaseContainer, processor, new ProcessorOptions());
        }

        public void Cancel()    
        {
            this.StopAsync(CancellationToken.None).Wait();
        }

        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this.changeProcessor.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this.changeProcessor.StopAsync();
        }
    }
}