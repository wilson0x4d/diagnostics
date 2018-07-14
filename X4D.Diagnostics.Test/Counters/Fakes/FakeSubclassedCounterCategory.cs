namespace X4D.Diagnostics.Counters.Fakes
{
    public sealed class FakeSubclassedCounterCategory :
        FakeUnsealedCounterCategory
    {
        public readonly SumTotal InTotal;

        public FakeSubclassedCounterCategory(string name)
            : base(name)
        {
            InTotal = new SumTotal();
        }
    }
}
