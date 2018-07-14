using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class MovingAverageTests
    {
        [TestMethod]
        public void MovingAverage_BasicVerification()
        {
            var seriesLength = byte.MaxValue;
            var counter = new MovingAverage(seriesLength);
            var series = new Queue<long>();
            var approximation = 0L;
            foreach (var count in FibonacciHelper.ComputeTo(47))
            {
                counter.Increment(count);
                series.Enqueue(count);
                approximation += count;
                if (series.Count > seriesLength)
                {
                    approximation -= series.Dequeue();
                }
                Assert.AreEqual(approximation / seriesLength, counter.Value);
            }
        }
    }
}
