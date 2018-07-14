using System;
using System.Diagnostics;

namespace X4D.Diagnostics.Counters.Fakes
{
    public sealed class FakeCounterCategory :
        CounterCategoryBase<FakeCounterCategory>,
        ISupportsReset
    {
        public readonly SumTotal InTotal;

        public readonly RatePerSecond InPerSec;

        public readonly SumTotal OutTotal;

        public readonly RatePerSecond OutPerSec;

        public readonly Delta PendingCount;

        public readonly MovingAverage PendingTimeAverage;

        public readonly SumTotal ErrorsTotal;

        public readonly RatePerSecond ErrorsPerSec;

        public readonly MeanAverage ErrorRatio;

        public FakeCounterCategory(string name)
            : base(name)
        {
            var inSyncRoot = new object();

            InTotal = new SumTotal(inSyncRoot);
            InPerSec = new RatePerSecond(InTotal, inSyncRoot);

            var outSyncRoot = new object();

            OutTotal = new SumTotal(outSyncRoot);
            OutPerSec = new RatePerSecond(OutTotal, outSyncRoot);
            PendingCount = new Delta(
                InTotal,
                OutTotal,
                outSyncRoot);
            PendingTimeAverage = new MovingAverage(
                syncRoot: outSyncRoot);

            var errorsSyncRoot = new object();

            ErrorsTotal = new SumTotal(errorsSyncRoot);
            ErrorsPerSec = new RatePerSecond(ErrorsTotal, errorsSyncRoot);
            ErrorRatio = new MeanAverage(
                ErrorsTotal,
                InTotal,
                errorsSyncRoot);
        }

        public void Reset()
        {
            InTotal.Reset();
            InPerSec.Reset();
            OutTotal.Reset();
            OutPerSec.Reset();
            PendingCount.Reset();
            PendingTimeAverage.Reset();
            ErrorsTotal.Reset();
            ErrorsPerSec.Reset();
            ErrorRatio.Reset();
        }

        public void Observe(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            InPerSec.Increment();
            try
            {
                action();
            }
            finally
            {
                if (System.Runtime.InteropServices.Marshal.GetExceptionCode() != 0)
                {
                    ErrorsPerSec.Increment();
                }
                PendingTimeAverage.Increment(stopwatch);
                OutPerSec.Increment();
            }
        }
    }
}
