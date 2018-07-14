using Microsoft.VisualStudio.TestTools.UnitTesting;
using X4D.Diagnostics.Counters.Fakes;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class CounterCategoryFactoryTests
    {
        [TestMethod]
        public void CounterCategoryFactory_GetInstance_UniqueByName()
        {
            var notExpected = CounterCategoryFactory<FakeCounterCategory>.GetInstance("category1");
            var actual = CounterCategoryFactory<FakeCounterCategory>.GetInstance("category2");
            Assert.AreNotEqual(notExpected, actual);
        }

        [TestMethod]
        public void CounterCategoryFactory_GetInstance_CachedByName()
        {
            var expected = CounterCategoryFactory<FakeCounterCategory>.GetInstance("category1");
            var actual = CounterCategoryFactory<FakeCounterCategory>.GetInstance("category1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CounterCategoryFactory_GetInstance_UniqueByType()
        {
            var notExpected = CounterCategoryFactory<FakeCounterCategory>.GetInstance("category1");
            var actual = CounterCategoryFactory<FakeCounterCategory2>.GetInstance("category1");
            Assert.AreNotEqual(notExpected, actual);
        }
    }
}
