namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    using System;

    public class ProcessorOptions
    {
        public TimeSpan PollDelay { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);
    }
}