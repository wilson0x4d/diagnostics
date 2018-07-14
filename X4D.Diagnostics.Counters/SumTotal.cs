namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates a Sum Total value.
    /// </summary>
    public class SumTotal :
        ISupportsIncrement<long>,
        ISupportsDecrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// the current Sum Total value.
        /// </summary>
        private long _value;

        /// <summary>
        /// Constructs a <see cref="SumTotal"/> instance.
        /// </summary>
        public SumTotal(
            object syncRoot = null)
        {
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets the current Sum Total value.
        /// </summary>
        public long Value => _value;

        public long Decrement(long value)
        {
            lock (_lock)
            {
                _value -= value;
                return _value;
            }
        }

        public long Increment(long value)
        {
            lock (_lock)
            {
                _value += value;
                return _value;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _value = 0;
            }
        }
    }
}
