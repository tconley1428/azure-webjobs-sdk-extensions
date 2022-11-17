namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILeaseContainer<TPartition, TLease, TContinuation>
        where TPartition : IPartition
        where TLease : ILease<TContinuation>
    {
        Task<bool> LockAsync();
        Task<bool> UnlockAsync();
        Task<bool> IsInitializedAsync();
        Task MarkInitializedAsync();

        Task<IEnumerable<TLease>> GetAllLeasesAsync();

        Task CreateLeaseAsync(TPartition partition);

        Task<Tuple<bool, TLease>> TakeLeaseAsync(TLease lease);

        Task<Tuple<bool, TLease>> ReleaseLeaseAsync(TLease lease);

        Task<Tuple<bool, TLease>> UpdateLeaseAsync(TLease lease);
    }
}