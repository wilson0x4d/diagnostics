using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class MedianAverageTests
    {
        [TestMethod]
        public void MedianAverage_BasicVerification()
        {
            var counter = new MedianAverage();

            // verify 'tween' result
            // with initial series: 1,1,2,3,5,8,13,21,34,55,89 !,! 144,233,377,610,987,1597,2584,4181,6765,10946,17711
            foreach (var count in FibonacciHelper.ComputeTo(21))
            {
                counter.Increment(count);
            }
            Assert.AreEqual(72, counter.Value);

            // verify 'non-tween' result
            // with additional series: 34,55,89,144,233,377, !610! ,987,1597,2584,4181,6765,10946,17711
            foreach (var count in FibonacciHelper.ComputeTo(27).Skip(8))
            {
                counter.Increment(count);
            }
            Assert.AreEqual(377, counter.Value);

            // verify performant enough not to timeout test tool
            var fib = FibonacciHelper.ComputeTo(127).ToArray();
            for (int i = 0; i < ushort.MaxValue; i++)
            {
                counter.Increment(fib[i++ % fib.Length]);
            }
            Assert.AreEqual(4807526976, counter.Value);

            // confirm outlier does not pull average
            counter.Reset();
            counter.Increment(1);
            counter.Increment(1);
            counter.Increment(1);
            counter.Increment(int.MaxValue);
            Assert.AreEqual(1, counter.Value);
        }
    }
}
