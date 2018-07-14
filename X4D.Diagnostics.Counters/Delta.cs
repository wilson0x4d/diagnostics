namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates the difference between two counters, referred to as the
    /// <see cref="Minuend"/> and the <see cref="Subtrahend"/>, as in "Value=Minuend-Subtrahend".
    /// <para>
    /// No operation executed on this object is performed against the <see cref="Subtrahend"/>.
    /// </para>
    /// <para>
    /// Every operation executed on this object is performed against the <see
    /// cref="Minuend"/> such that <see cref="Delta"/> can be substituted for
    /// the Minuend elsewhere in your code.
    /// </para>
    /// </summary>
    public class Delta :
        ICounter<long>,
        ISupportsIncrement<long>,
        ISupportsDecrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// the Minuend.
        /// </summary>
        private readonly ICounter<long> _minuend;

        /// <summary>
        /// the Subtrahend.
        /// </summary>
        private readonly ICounter<long> _subtrahend;

        /// <summary>
        /// Constructs a <see cref="Delta"/> instance using a <see
        /// cref="SumTotal"/> minuend and <see cref="SumTotal"/> subtrahend.
        /// </summary>
        public Delta(
            object syncRoot = null)
            : this(new SumTotal(syncRoot))
        { }

        /// <summary>
        /// Constructs a <see cref="Delta"/> instance using the
        /// caller-specified <paramref name="subtrahend"/> and a <see
        /// cref="SumTotal"/> minuend.
        public Delta(
            ICounter<long> subtrahend,
            object syncRoot = null)
            : this(subtrahend, new SumTotal(syncRoot), syncRoot)
        { }

        /// <summary>
        /// Constructs a <see cref="Delta"/> instance using the
        /// caller-specified <paramref name="subtrahend"/> and <paramref
        /// name="minuend"/>.
        /// </summary>
        public Delta(
            ICounter<long> subtrahend,
            ICounter<long> minuend,
            object syncRoot = null)
        {
            _subtrahend = subtrahend;
            _minuend = minuend;
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets the Minuend Counter.
        /// </summary>
        public ICounter<long> Minuend => _minuend;

        /// <summary>
        /// Gets the Subtrahend Counter.
        /// </summary>
        public ICounter<long> Subtrahend => _subtrahend;

        /// <summary>
        /// Gets the Difference of <see cref="Minuend"/> and <see cref="Subtrahend"/>.
        /// </summary>
        public long Value => _minuend.Value - _subtrahend.Value;

        /// <summary>
        /// Decrement the <see cref="Minuend"/> by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to Decrement the Counter by.</param>
        /// <returns>The resulting Counter Value after Decrementing.</returns>
        public long Decrement(
            long value)
        {
            return (_minuend as ISupportsDecrement<long>)
                ?.Decrement(value)
                ?? _minuend.Value;
        }

        /// <summary>
        /// Increment the <see cref="Minuend"/> by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to Increment the Counter by.</param>
        /// <returns>The resulting Counter Value after Incrementing.</returns>
        public long Increment(
            long value)
        {
            return (_minuend as ISupportsIncrement<long>)
                ?.Increment(value)
                ?? _minuend.Value;
        }

        /// <summary>
        /// Reset Counter State.
        /// <para>
        /// Calls <see cref="ISupportsReset.Reset"/> on both <see
        /// cref="Minuend"/> and <see cref="Subtrahend"/>.
        /// </para>
        /// </summary>
        public void Reset()
        {
            (_minuend as ISupportsReset)?.Reset();
            (_subtrahend as ISupportsReset)?.Reset();
        }
    }
}
