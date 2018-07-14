using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace X4D.Diagnostics.Fakes
{
    public sealed class FakeTraceListener :
        TraceListener
    {
        private readonly StringBuilder _output;

        public FakeTraceListener()
        {
            _output = new StringBuilder();
        }

        public override void Write(string message)
        {
            _output.Append(message);
        }

        public override void WriteLine(string message)
        {
            _output.AppendLine(message);
        }

        internal string GetLogOutput()
        {
            return _output.ToString();
        }
    }
}
