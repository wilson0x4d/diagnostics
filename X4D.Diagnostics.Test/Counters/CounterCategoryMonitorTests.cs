using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using X4D.Diagnostics.Counters.Fakes;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class CounterCategoryMonitorTests
    {
        [TestMethod]
        public void CounterCategoryMonitor_Publish_BasicVerification()
        {
            var counterCategory = GetType().GetCounterCategory<FakeCounterCategory>();
            using (var signal500 = new ManualResetEvent(false))
            using (var signal1000 = new ManualResetEvent(false))
            {
                var snapshot500 = default(IDictionary<string, object>);
                var snapshot1000 = default(IDictionary<string, object>);
                counterCategory.Monitor.AddObserver(
                    (L_snapshot) =>
                    {
                        Assert.IsNotNull(L_snapshot);
                        snapshot500 = L_snapshot;
                        signal500.Set();
                    },
                    TimeSpan.FromMilliseconds(500));
                counterCategory.Monitor.AddObserver(
                    (L_snapshot) =>
                    {
                        Assert.IsNotNull(L_snapshot);
                        snapshot1000 = L_snapshot;
                        signal1000.Set();
                    },
                    TimeSpan.FromMilliseconds(1000));
                counterCategory.Observe(() =>
                {
                    snapshot500 = null;
                    signal500.Reset();
                    Assert.IsTrue(signal500.WaitOne(5000));
                    Assert.AreEqual(1, Convert.ToInt64(snapshot500["InTotal"]));
                });
                counterCategory.Observe(() =>
                {
                    snapshot1000 = null;
                    signal1000.Reset();
                    Assert.IsTrue(signal1000.WaitOne(5000));
                    Assert.AreEqual(2, Convert.ToInt64(snapshot1000["InTotal"]));
                    Assert.AreEqual(1, Convert.ToInt64(snapshot1000["OutTotal"]));
                });
            }
        }

        [TestMethod]
        public void CounterCategoryMonitor_RemoveObserver_BasicVerification()
        {
            var counterCategory = GetType().GetCounterCategory<FakeCounterCategory>();
            using (var signal = new ManualResetEvent(false))
            {
                var observer = (Action<IDictionary<string, object>>)delegate (IDictionary<string, object> snapshot)
                {
                    signal.Set();
                };
                counterCategory.Monitor.AddObserver(observer, TimeSpan.FromMilliseconds(100));
                counterCategory.Monitor.RemoveObserver(observer);
                signal.Reset();
                if (signal.WaitOne(500))
                {
                    Assert.Fail($"Observer signal received, therefore was not removed as expected.");
                }

                // quick verification that attempted removals of already removed and/or from empty monitors will not throw.
                counterCategory.Monitor.RemoveObserver(observer);
            }
        }

        [TestMethod]
        public void CounterCategoryMonitor_ctor_EmptyCategoryWillThrow()
        {
            try
            {
                var actual = CounterCategoryFactory<FakeEmptyCounterCategory>.GetInstance("category1");
                Assert.Fail("CounterCategoryMonitor is accepting empty types, which is not supported.");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsTrue(ex.InnerException?.Message?.Contains("does not expose any `public readonly` counters") == true);
            }
        }
    }
}
