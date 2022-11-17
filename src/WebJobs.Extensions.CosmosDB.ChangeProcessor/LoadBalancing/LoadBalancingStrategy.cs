//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.ChangeProcessor.LoadBalancing
{
    using System.Collections.Generic;

    internal abstract class LoadBalancingStrategy<TLease, TContinuation>
        where TLease : ILease<TContinuation>
    {
        /// <summary>
        /// Select leases that should be taken for processing.
        /// This method will be called periodically with <see cref="ChangeFeedLeaseOptions.LeaseAcquireInterval"/>
        /// </summary>
        /// <param name="allLeases">All leases</param>
        /// <returns>Leases that should be taken for processing by this host</returns>
        public abstract IEnumerable<TLease> SelectLeasesToTake(IEnumerable<TLease> allLeases);
    }
}