namespace X4D.Diagnostics.Counters.Fakes
{
    public sealed class FakeEmptyCounterCategory :
        CounterCategoryBase<FakeEmptyCounterCategory>
    {
        public FakeEmptyCounterCategory(string name)
            : base(name)
        {
        }
    }
}
