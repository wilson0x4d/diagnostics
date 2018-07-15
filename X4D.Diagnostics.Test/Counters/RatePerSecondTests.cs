using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class RatePerSecondTests
    {
        [TestMethod]
        public void RatePerSecond_BasicVerification()
        {
            var counter = new RatePerSecond();
            foreach (var item in FibonacciHelper.ComputeTo(47).Skip(7).Take(7))
            {
                counter.Increment(item);
            }
            Assert.AreEqual(589, counter.Value);
        }
    }
}
