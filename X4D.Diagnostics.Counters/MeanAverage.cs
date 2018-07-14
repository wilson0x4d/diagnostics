namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates the Mean Average of a <see cref="Numerator"/> and <see cref="Denominator"/>.
    /// <para>
    /// No operation executed on this object is performed against the <see cref="Denominator"/>.
    /// </para>
    /// <para>
    /// Every operation executed on this object is performed against the <see
    /// cref="Numerator"/> such that <see cref="MeanAverage"/> can be substituted for
    /// the Numerator elsewhere in your code.
    /// </para>
    /// </summary>
    public class MeanAverage :
        ISupportsIncrement<long>,
        ISupportsDecrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// the Numerator.
        /// </summary>
        private readonly ICounter<long> _numerator;

        /// <summary>
        /// the Denominator.
        /// </summary>
        private readonly ICounter<long> _denominator;

        /// <summary>
        /// Constructs a <see cref="MeanAverage"/> instance with a <see
        /// cref="SumTotal"/> numerator and <see cref="SumTotal"/> denominator.
        /// </summary>
        public MeanAverage(
            object syncRoot = null)
            : this(new SumTotal(syncRoot), syncRoot)
        { }

        /// <summary>
        /// Constructs a <see cref="MeanAverage"/> with a <see
        /// cref="SumTotal"/> numerator and the specified <paramref name="denominator"/>.
        /// </summary>
        /// <param name="denominator"></param>
        public MeanAverage(
            ICounter<long> denominator,
            object syncRoot = null)
            : this(denominator, new SumTotal(syncRoot), syncRoot)
        { }

        /// <summary>
        /// Constructs a <see cref="MeanAverage"/> with the specified
        /// <paramref name="numerator"/> and <paramref name="denominator"/>.
        /// </summary>
        public MeanAverage(
            ICounter<long> denominator,
            ICounter<long> numerator,
            object syncRoot = null)
        {
            _denominator = denominator;
            _numerator = numerator;
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets the Numerator Counter.
        /// </summary>
        public ICounter<long> Numerator => _numerator;

        /// <summary>
        /// Gets the Denominator Counter.
        /// </summary>
        public ICounter<long> Denominator => _denominator;

        /// <summary>
        /// Gets the Mean Value of the Numerator and Denominator.
        /// </summary>
        public virtual long Value
        {
            get
            {
                var baseValue = _denominator.Value;
                return baseValue > 0
                    ? _numerator.Value / baseValue
                    : _numerator.Value;
            }
        }

        public long Decrement(long value)
        {
            if (_numerator is ISupportsDecrement<long> numerator)
            {
                numerator.Decrement(value);
            }
            return Value;
        }

        public long Increment(long value)
        {
            if (_numerator is ISupportsIncrement<long> numerator)
            {
                numerator.Increment(value);
            }
            return Value;
        }

        /// <summary>
        /// Reset Counter State.
        /// <para>
        /// Calls <see cref="ISupportsReset.Reset"/> on both <see
        /// cref="Numerator"/> and <see cref="Denominator"/>.
        /// </para>
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                (_numerator as ISupportsReset)?.Reset();
                (_denominator as ISupportsReset)?.Reset();
            }
        }
    }
}
