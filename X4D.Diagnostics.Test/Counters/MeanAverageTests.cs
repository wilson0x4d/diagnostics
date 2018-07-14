using Microsoft.VisualStudio.TestTools.UnitTesting;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class MeanAverageTests
    {
        [TestMethod]
        public void MeanAverage_BasicVerification()
        {
            var counter = new MeanAverage();
            var expectedCount = 0L;
            var expectedTotalCounts = 0L;
            foreach (var count in FibonacciHelper.ComputeTo(47))
            {
                counter.Increment(count);
                (counter.Denominator as ISupportsIncrement<long>)?.Increment();
                expectedCount += count;
                expectedTotalCounts += 1;
                Assert.AreEqual(expectedCount / expectedTotalCounts, counter.Value);
            }
        }
    }
}
