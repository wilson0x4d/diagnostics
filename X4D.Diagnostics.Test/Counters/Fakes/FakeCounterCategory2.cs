namespace X4D.Diagnostics.Counters.Fakes
{
    public sealed class FakeCounterCategory2 :
        CounterCategoryBase<FakeCounterCategory2>,
        ISupportsReset
    {
        public readonly RatePerSecond InPerSec;

        public FakeCounterCategory2(string name)
            : base(name)
        {
            InPerSec = new RatePerSecond();
        }

        public void Reset()
        {
            InPerSec.Reset();
        }
    }
}
