using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using X4D.Diagnostics.Configuration;
using X4D.Diagnostics.Fakes;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.Logging
{
    [TestClass]
    public class LoggingExtensionsTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // HACK: because test engine does not open correct config we load
            //       correct config manually. once test containers properly
            //       load their config this should be removed.
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap
                {
                    ExeConfigFilename = $"{Assembly.GetExecutingAssembly().Location}.config"
                },
                ConfigurationUserLevel.None);
            var configurationSection = configuration.GetSection("system.diagnostics") as SystemDiagnosticsConfigurationSection;
        }

        [TestMethod]
        public void LoggingExtensions_Exception_CanLog()
        {
            var expectedMessage = Guid.NewGuid().ToString();
            try
            {
                throw new FakeException(expectedMessage);
            }
            catch (FakeException ex)
            {
                Assert.AreEqual(expectedMessage, ex.Message);
                ex.Log();
            }
            // verify
            foreach (var traceListener in Trace.Listeners)
            {
                if (traceListener is FakeTraceListener fakeTraceListener)
                {
                    if (fakeTraceListener.Name.Equals(@"TestLog", StringComparison.OrdinalIgnoreCase))
                    {
                        var actualOutput = fakeTraceListener.GetLogOutput();
                        Assert.IsTrue(actualOutput.Contains(expectedMessage));
                        return;
                    }
                }
            }
            Assert.Fail("Could not verify trace listener.");
        }

        [TestMethod]
        public void LoggingExtensions_String_CanLog()
        {
            var expectedMessage = Guid.NewGuid().ToString();
            expectedMessage.Log();
            // verify
            foreach (var traceListener in Trace.Listeners)
            {
                if (traceListener is FakeTraceListener fakeTraceListener)
                {
                    if (fakeTraceListener.Name.Equals(@"TestLog", StringComparison.OrdinalIgnoreCase))
                    {
                        var actualOutput = fakeTraceListener.GetLogOutput();
                        Assert.IsTrue(actualOutput.Contains(expectedMessage));
                        return;
                    }
                }
            }
            Assert.Fail("Could not verify trace listener.");
        }

        [TestMethod]
        public void LoggingExtensions_StringBuilder_CanLog()
        {
            var expectedMessage = Guid.NewGuid().ToString();
            var stringBuilder = new StringBuilder(expectedMessage);
            stringBuilder.Log();
            // verify
            foreach (var traceListener in Trace.Listeners)
            {
                if (traceListener is FakeTraceListener fakeTraceListener)
                {
                    if (fakeTraceListener.Name.Equals(@"TestLog", StringComparison.OrdinalIgnoreCase))
                    {
                        var actualOutput = fakeTraceListener.GetLogOutput();
                        Assert.IsTrue(actualOutput.Contains(expectedMessage));
                        return;
                    }
                }
            }
            Assert.Fail("Could not verify trace listener.");
        }

        [TestMethod]
        public void LoggingExtensions_TextReader_CanLog()
        {
            var expectedMessage = Guid.NewGuid().ToString();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            using (var reader = new StreamReader(stream))
            {
                writer.Write(expectedMessage);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                reader.Log();
            }
            // verify
            foreach (var traceListener in Trace.Listeners)
            {
                if (traceListener is FakeTraceListener fakeTraceListener)
                {
                    if (fakeTraceListener.Name.Equals(@"TestLog", StringComparison.OrdinalIgnoreCase))
                    {
                        var actualOutput = fakeTraceListener.GetLogOutput();
                        Assert.IsTrue(actualOutput.Contains(expectedMessage));
                        return;
                    }
                }
            }
            Assert.Fail("Could not verify trace listener.");
        }
    }
}
