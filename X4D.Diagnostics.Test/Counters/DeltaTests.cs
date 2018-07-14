using Microsoft.VisualStudio.TestTools.UnitTesting;
using X4D.Diagnostics.Counters;

namespace X4D.Diagnostics.Counters
{
    [TestClass]
    public class DeltaTests
    {
        [TestMethod]
        public void Delta_Value_DoesCalculateDifferences()
        {
            var delta = new Delta();
            // verify delta Value
            (delta.Subtrahend as ISupportsDecrement<long>).Decrement(5);
            (delta.Minuend as ISupportsIncrement<long>).Increment(5);
            Assert.AreEqual(10, delta.Value);

            // verify change affects delta Value
            for (int i = 10; i > -10; i--)
            {
                Assert.AreEqual(i, delta.Value);
                delta.Decrement();
            }
        }
    }
}
