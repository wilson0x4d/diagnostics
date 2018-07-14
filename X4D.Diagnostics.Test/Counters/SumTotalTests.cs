using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class SumTotalTests
    {
        [TestMethod]
        public void SumTotal_Decrement_IsThreadSafe()
        {
            var counter = new SumTotal();
            var cancellationTokenSource = new CancellationTokenSource();
            var tasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                counter.Increment(100000);
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    while (counter.Decrement() >= Environment.ProcessorCount)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }, cancellationTokenSource.Token));
            }
            if (!Task.WaitAll(tasks.ToArray(), 3000, cancellationTokenSource.Token))
            {
                cancellationTokenSource.Cancel(false);
                Task.WaitAll(tasks.ToArray());
                Assert.Fail("Counter tasks timed out.");
            }
            Assert.AreEqual(0, counter.Value);
        }

        [TestMethod]
        public void SumTotal_Increment_IsThreadSafe()
        {
            var expectedCounts = 100000 * Environment.ProcessorCount;
            var counter = new SumTotal();
            var cancellationTokenSource = new CancellationTokenSource();
            var tasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    while (counter.Increment() <= (expectedCounts - Environment.ProcessorCount))
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    };
                }, cancellationTokenSource.Token));
            }
            if (!Task.WaitAll(tasks.ToArray(), 3000, cancellationTokenSource.Token))
            {
                cancellationTokenSource.Cancel(false);
                Task.WaitAll(tasks.ToArray());
                Assert.Fail("Counter tasks timed out.");
            }
            Assert.AreEqual(expectedCounts, counter.Value);
        }
    }
}
