using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class CompositeCounterTests
    {
        [TestMethod]
        public void CompositeCounter_BasicVerification()
        {
            var sumTotal = new SumTotal();
            var ratePerSec = new RatePerSecond();
            var meanPerSec =
                new MeanAverage(
                    ratePerSec.Denominator);
            var medianPerSec = new MedianAverage();
            var movingAverage = new MovingAverage(7);
            var composite = new CompositeCounter()
                .AddCounter(sumTotal)
                .AddCounter(ratePerSec)
                .AddCounter(meanPerSec)
                .AddCounter(medianPerSec)
                .AddCounter(movingAverage);

            var expectedSumTotal = 0L;
            var elapsedTime = new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Milliseconds);
            while (elapsedTime.Value <= 500)
            {
                Thread.Sleep(50);
                expectedSumTotal++;
                composite.Increment(1);
            }
            Thread.Sleep(500);
            Assert.AreEqual(expectedSumTotal, sumTotal.Value);
            Assert.AreEqual(10 /* == 1 per 50ms for 500ms */, ratePerSec.Value);
            Assert.AreEqual(sumTotal.Value, composite.Value);
        }
    }
}
