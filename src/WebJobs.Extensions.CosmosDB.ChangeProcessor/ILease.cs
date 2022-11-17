using System;

namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    public interface ILease<TContinuation>
    {
        DateTime Timestamp();
        string Owner();
        string Id();
        TContinuation Continuation();
        void SetContinuation(TContinuation newContinuation);
        void SetOwner(string owner);
        void SetTimestamp(DateTime dateTime);
    }
}
