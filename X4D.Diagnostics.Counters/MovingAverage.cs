namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates a Mean Average from a Finite Set, where new values cause
    /// older values to evict from the set (FIFO.)
    /// </summary>
    public class MovingAverage :
        ISupportsIncrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// the Finite Set of numbers used to calculate a Moving Average.
        /// </summary>
        private readonly long[] _set;

        /// <summary>
        /// the Write Index for the Set.
        /// </summary>
        private byte _writeIndex = 0;

        /// <summary>
        /// the Value representing the Mean Average of the Set.
        /// </summary>
        private long _value;

        /// <summary>
        /// Constructs a <see cref="MovingAverage"/> instance with a set
        /// length of "7" elements.
        /// </summary>
        /// <param name="setLength">
        /// The max number of elements to allow in the Set.
        /// </param>
        public MovingAverage(
            byte setLength = 7,
            object syncRoot = null)
        {
            if (setLength <= 2)
            {
                setLength = 2;
            }
            _set = new long[setLength];
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets the Mean Average value of the current Set.
        /// </summary>
        public long Value
        {
            get
            {
                return _value / _set.Length;
            }
        }

        /// <summary>
        /// Assigns the specified <paramref name="value"/> to the Set.
        /// <para>
        /// "Increment" is a bit of a misnomer, but <see
        /// cref="ISupportsIncrement{T}"/> is a valuable interface to expose
        /// here so that this counter can be assigned as a Base Counter of
        /// other counters and still participate as one might expect.
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Increment(long value)
        {
            lock (_lock)
            {
                var writeIndex = _writeIndex % _set.Length;
                var exists = _set[writeIndex];
                if (exists != 0)
                {
                    _value -= exists;
                }
                _value += value;
                _set[writeIndex] = value;
                unchecked
                {
                    _writeIndex++;
                }
                return Value;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _value = 0;
                _writeIndex = 0;
                for (int i = 0; i < _set.Length; i++)
                {
                    _set[i] = 0L;
                }
            }
        }
    }
}
