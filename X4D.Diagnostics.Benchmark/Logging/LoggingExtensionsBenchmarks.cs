using BenchmarkDotNet.Attributes;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.Benchmark.Logging
{
    public class LoggingExtensionsBenchmarks
    {
        [Benchmark]
        public void Exception_Baseline_Cost()
        {
            try
            {
                throw new Exception("expected");
            }
            catch (Exception)
            {
                // NOP
            }
        }

        [Benchmark]
        public void Exception_Log_Cost()
        {
            try
            {
                throw new Exception("expected");
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        [Benchmark]
        public void String_Log_Cost()
        {
            "expected".Log();
        }

        [Benchmark]
        public void StringBuilder_Log_Cost()
        {
            new StringBuilder("expected").Log();
        }

        [Benchmark]
        public void StringBuilder_Baseline_Cost()
        {
            var sb = new StringBuilder("");
        }

        [Benchmark]
        public void TextReader_Log_Cost()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("expected"));
            using (var reader = new StreamReader(stream))
            {
                reader.Log();
            }
        }

        [Benchmark]
        public void TextReader_Baseline_Cost()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("expected"));
            using (var reader = new StreamReader(stream))
            {
                // NOP
            }
        }

        #region BCL Baselines

        [Benchmark]
        public void Diagnostics_StackTrace_Cost()
        {
            Diagnostics_StackTrace_Cost_INNER();
        }

        public void Diagnostics_StackTrace_Cost_INNER()
        {
            var stackTrace = new StackTrace();
            Trace.WriteLine(stackTrace.GetFrame(1).GetMethod().Name);
        }

        [Benchmark]
        public void Diagnostics_StackFrame_Cost()
        {
            Diagnostics_StackFrame_Cost_INNER();
        }

        public void Diagnostics_StackFrame_Cost_INNER()
        {
            Trace.WriteLine(new StackFrame(1).GetMethod().Name);
        }

        [Benchmark]
        public void Diagnostics_CallerName_Cost()
        {
            Diagnostics_CallerName_Cost_INNER("test");
        }

        public void Diagnostics_CallerName_Cost_INNER(
            [CallerMemberName] string callerName = "")
        {
            Trace.WriteLine(callerName);
        }

        #endregion
    }
}
