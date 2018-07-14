using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using X4D.Diagnostics.Counters.Fakes;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class CounterCategoryBaseTests
    {
        [TestMethod]
        public void CounterCategoryBase_ctor_UnsealedTypeWillThrow()
        {
            try
            {
                var actual = CounterCategoryFactory<FakeUnsealedCounterCategory>.GetInstance("category1");
                Assert.Fail("CounterCategoryBase is not preventing unsealed types, which is not supported.");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsNotNull(ex.InnerException as NotSupportedException);
                Assert.IsTrue(ex.InnerException.Message?.Contains("must be a sealed type") == true);
            }
        }

        [TestMethod]
        public void CounterCategoryBase_ctor_IndirectSubclassWillThrow()
        {
            try
            {
                var actual = new FakeSubclassedCounterCategory("category1");
                Assert.Fail("CounterCategoryBase is not preventing subclassed types, which is not supported.");
            }
            catch (NotSupportedException ex)
            {
                Assert.IsTrue(ex.Message?.Contains($"does not directly inherit '{typeof(CounterCategoryBase<>).Name}'") == true);
            }
        }

        [TestMethod]
        public void CounterCategoryBase_ctor_IncorrectTypeParameterWillThrow()
        {
            try
            {
                var actual = new FakeIncorrectTypeParamCounterCategory("category1");
                Assert.Fail("CounterCategoryBase is not preventing incorrect type param, which is not supported.");
            }
            catch (NotSupportedException ex)
            {
                Assert.IsTrue(ex.Message?.Contains($"specifies an incorrect type parameter") == true);
            }
        }
    }
}
