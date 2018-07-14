using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class ElapsedTimeTests
    {
        [TestMethod]
        public void ElapsedTime_Value_AccurateWithin100ms()
        {
            var stopwatch = Stopwatch.StartNew();
            var counter = new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Milliseconds);
            Thread.Sleep(1100);
            var stopwatchMilliseconds = stopwatch.ElapsedMilliseconds;
            var counterMilliseconds = counter.Value;
            Assert.IsTrue(counterMilliseconds <= 1200);
            Assert.IsTrue(Math.Abs(stopwatchMilliseconds - stopwatchMilliseconds) <= 100);
        }

        [TestMethod]
        public void ElapsedTime_Value_AccurateWithin10ms()
        {
            var stopwatch = Stopwatch.StartNew();
            var counter = new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Milliseconds);
            Thread.Sleep(1010);
            var stopwatchMilliseconds = stopwatch.ElapsedMilliseconds;
            var counterMilliseconds = counter.Value;
            Assert.IsTrue(counterMilliseconds <= 1020);
            Assert.IsTrue(Math.Abs(stopwatchMilliseconds - stopwatchMilliseconds) <= 10);
        }

        [TestMethod]
        public void ElapsedTime_Value_AccurateWithin1ms()
        {
            var stopwatch = Stopwatch.StartNew();
            var counter = new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Milliseconds);
            Thread.Sleep(1001);
            var stopwatchMilliseconds = stopwatch.ElapsedMilliseconds;
            var counterMilliseconds = counter.Value;
            Assert.IsTrue(counterMilliseconds <= 1002);
            Assert.IsTrue(Math.Abs(stopwatchMilliseconds - stopwatchMilliseconds) <= 1);
        }
    }
}
