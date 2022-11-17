using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Cosmos.ChangeProcessor
{
    internal static class Trace
    {
        private static TraceSource traceSource = new TraceSource("CosmosChangeProcessor");

        public static void Information(string message, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Information, 0, message, args);
        }
    }
}
