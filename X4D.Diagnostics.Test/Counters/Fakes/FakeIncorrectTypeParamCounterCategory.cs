namespace X4D.Diagnostics.Counters.Fakes
{
    public sealed class FakeIncorrectTypeParamCounterCategory :
        CounterCategoryBase<FakeCounterCategory>
    {
        public readonly RatePerSecond OutPerSec;

        public FakeIncorrectTypeParamCounterCategory(string name)
            : base(name)
        {
            OutPerSec = new RatePerSecond();
        }
    }
}
