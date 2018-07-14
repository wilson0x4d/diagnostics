namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Provides a rate of values per second.
    /// <para>
    /// This is simply a <see cref="MeanAverage"/> subclass coded to use a
    /// <see cref="SumTotal"/> numerator and <see cref="ElapsedTime"/> denominator.
    /// </para>
    /// <para>
    /// No behaviors of the <see cref="MeanAverage"/> superclass are modified.
    /// </para>
    /// </summary>
    public class RatePerSecond :
        MeanAverage
    {
        /// <summary>
        /// Constructs a <see cref="RatePerSecond"/> instance with a <see
        /// cref="SumTotal"/> numerator.
        /// </summary>
        public RatePerSecond(
            object syncRoot = null)
            : base(
                  new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Seconds),
                  new SumTotal(syncRoot),
                  syncRoot)
        { }

        /// <summary>
        /// Constructs a <see cref="RatePerSecond"/> instance with the
        /// specified <paramref name="numerator"/>.
        /// </summary>
        /// <param name="numerator"></param>
        public RatePerSecond(
            ICounter<long> numerator,
            object syncRoot = null)
            : base(
                  new ElapsedTime(ElapsedTime.ElapsedTimeUnitType.Seconds),
                  numerator,
                  syncRoot)
        { }
    }
}
