using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class ObservedValueTests
    {
        [TestMethod]
        public void ObservedValue_BasicVerification()
        {
            var minObservedValue = new ObservedValue(ObservationType.Minimum);
            Assert.AreEqual(0, minObservedValue.Value, "because an unused `ObservedValue` counter should not return a non-zero result.");
            minObservedValue.Observe(1);
            Assert.AreEqual(1, minObservedValue.Value, "because the first use of `ObservedValue` should always mutate the default state.");
            var maxObservedValue = new ObservedValue(ObservationType.Maximum);
            Assert.AreEqual(0, maxObservedValue.Value, "because an unused ObservedValue counter should not return a non-zero result.");
            maxObservedValue.Observe(-1);
            Assert.AreEqual(-1, maxObservedValue.Value, "because the first use of `ObservedValue` should always mutate the default state.");
            var lastObservedValue = new ObservedValue(ObservationType.Last);
            var composite = new CompositeCounter()
                .AddCounter(minObservedValue)
                .AddCounter(maxObservedValue)
                .AddCounter(lastObservedValue);

            var expectedlastValue = 0L;
            var expectedMinValue = 0L;
            var expectedMaxValue = 0L;
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            for (int i = -10; i <= 10; i++)
            {
                var buf = new byte[4];
                rng.GetNonZeroBytes(buf);
                expectedlastValue = System.BitConverter.ToInt32(buf) * i;
                if (expectedlastValue < expectedMinValue)
                {
                     expectedMinValue = expectedlastValue;
                }
                if (expectedlastValue > expectedMaxValue)
                {
                    expectedMaxValue = expectedlastValue;
                }
                composite.Increment(expectedlastValue);
            }
            Assert.AreEqual(expectedlastValue, lastObservedValue.Value);
            Assert.AreEqual(expectedMinValue, minObservedValue.Value);
            Assert.AreEqual(expectedMaxValue, maxObservedValue.Value);
        }
    }
}
