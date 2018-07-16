using System;
using System.Diagnostics;
using System.IO;

namespace X4D.Diagnostics.TraceListeners
{
    public class ConsoleTraceListener : 
        TextWriterTraceListener
    {
        public ConsoleTraceListener()
            : base(Console.Out)
        {
        }

        public ConsoleTraceListener(Stream stream)
            : base(stream)
        {
        }
    }
}
