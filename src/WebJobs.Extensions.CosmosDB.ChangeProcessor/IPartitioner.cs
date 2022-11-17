namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IPartitioner<TPartition> 
        where TPartition : IPartition
    {
        Task<IEnumerable<TPartition>> GetPartitionsAsync();
    }
}
