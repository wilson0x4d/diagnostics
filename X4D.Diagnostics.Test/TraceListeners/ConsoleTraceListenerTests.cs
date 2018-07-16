using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace X4D.Diagnostics.TraceListeners
{
    [TestClass]
    public sealed class ConsoleTraceListenerTests
    {
        [TestMethod]
        public void ConsoleTraceListener_BasicVerification()
        {
            var expectedText = Guid.NewGuid().ToString();
            using (var memoryStream = new MemoryStream())
            using (var listener = new ConsoleTraceListener(memoryStream))
            {
                listener.WriteLine(expectedText);
                listener.Flush();
                var buf = memoryStream.ToArray();
                var actualText = Encoding.UTF8.GetString(buf, 0, buf.Length);
                Assert.IsTrue(actualText.Contains(expectedText));
            }
        }
    }
}
