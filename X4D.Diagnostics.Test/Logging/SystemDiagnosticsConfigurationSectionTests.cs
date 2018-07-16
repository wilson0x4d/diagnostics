using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text;
using X4D.Diagnostics.Fakes;

namespace X4D.Diagnostics.Configuration
{
    [TestClass]
    public class SystemDiagnosticsConfigurationSectionTests
    {
        [TestMethod]
        public void ConfigurationManager_GetSection_WillReturnSystemDiagnosticsSection()
        {
            var section = SystemDiagnosticsBootstrapper.Configure()
                as SystemDiagnosticsConfigurationSection;

            Assert.IsNotNull(section);
            // assert
            Assert.IsFalse(section.Assert.AssertUIEnabled);
            Assert.AreEqual("assert.log", section.Assert.LogFileName);
            // performanceCounters
            Assert.AreEqual(123456, section.PerfCounters.FileMappingSize);
            // sources
            Assert.AreEqual(3, section.Sources.Count);
            // trace
            Assert.AreEqual(true, section.Trace.AutoFlush);
            Assert.AreEqual(3, section.Trace.Listeners.Count);
            // sharedListeners
            Assert.AreEqual(3, section.SharedListeners.Count);
        }

        [TestMethod]
        public void SystemDiagnosticsBootstrapper_SystemDiagnosticsTrace_ShouldWriteToConfiguredTraceLog()
        {
            var expectedMessage = Guid.NewGuid().ToString();
            var stringBuilder = new StringBuilder(expectedMessage);

            Trace.Write(expectedMessage);

            // verify
            foreach (var traceListener in Trace.Listeners)
            {
                if (traceListener is FakeTraceListener fakeTraceListener)
                {
                    if (fakeTraceListener.Name.Equals(@"TraceLog", StringComparison.OrdinalIgnoreCase))
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
