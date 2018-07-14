using Microsoft.VisualStudio.TestTools.UnitTesting;
using X4D.Diagnostics.Counters.Fakes;
using X4D.Diagnostics.Fakes;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class CountersExtensionsTests
    {
        [TestMethod]
        public void CountersExtensions_GetInstance_BasicVerification()
        {
            var expectedCount = 100;
            var expectedErrors = 4;
            var expectedErrorRate = expectedCount / expectedErrors;

            var counters = GetType().GetCounterCategory<FakeCounterCategory>();
            counters.Reset();

            for (int i = 1; i <= expectedCount; i++)
            {
                try
                {
                    counters.Observe(() =>
                    {
                        if (0 == i % expectedErrorRate)
                        {
                            throw new FakeException();
                        }
                    });
                }
                catch (FakeException)
                {
                    // NOP
                }
            }

            Assert.AreEqual(expectedCount, counters.InTotal.Value);
            Assert.AreEqual(expectedCount, counters.InPerSec.Value);
            Assert.AreEqual(expectedCount, counters.OutTotal.Value);
            Assert.AreEqual(expectedCount, counters.OutPerSec.Value);
            Assert.AreEqual(expectedErrorRate, counters.ErrorRatio.Value);
            Assert.AreEqual(expectedErrors, counters.ErrorsTotal.Value);
            Assert.AreEqual(expectedErrors, counters.ErrorsPerSec.Value);
        }
    }
}
