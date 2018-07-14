using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using X4D.Diagnostics.Counters;

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
            Assert.AreEqual(953, counter.Value);
        }
    }
}
