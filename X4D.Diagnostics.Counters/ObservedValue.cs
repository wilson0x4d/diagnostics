namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Observes values via <see cref="ISupportsIncrement"/> and <see
    /// cref="ISupportsDecrement"/> interfaces, providing a value according
    /// to <see cref="ObservationType"/>.
    /// </summary>
    public class ObservedValue :
        ISupportsIncrement<long>,
        ISupportsDecrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// The type of observation to perform.
        /// </summary>
        private readonly ObservationType _observationType;

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
        public ObservedValue(
            ObservationType observationType,
            object syncRoot = null)
        {
            _observationType = observationType;
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets the Mean Average value of the current Set.
        /// </summary>
        public long Value
        {
            get
            {
                return _value;
            }
            set
            {
                Observe(value);
            }
        }

        /// <summary>
        /// Observes the supplied <paramref name="value"/>.
        /// <para>
        /// "Decrement" is a bit of a misnomer, but <see
        /// cref="ISupportsIncrement{T}"/> is a valuable interface to expose
        /// here so that this counter can be compisited with other counters
        /// and still participate as one might expect.
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Decrement(long value)
        {
            return Observe(value);
        }

        /// <summary>
        /// Observes the supplied <paramref name="value"/>.
        /// <para>
        /// "Increment" is a bit of a misnomer, but <see
        /// cref="ISupportsIncrement{T}"/> is a valuable interface to expose
        /// here so that this counter can be compisited with other counters
        /// and still participate as one might expect.
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Increment(long value)
        {
            return Observe(value);
        }

        /// <summary>
        /// Observes the supplied <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Observe(long value)
        {
            lock (_lock)
            {
                switch (_observationType)
                {
                    case ObservationType.Last:
                        _value = value;
                        break;

                    case ObservationType.Minimum:
                        if (value < _value)
                        {
                            _value = value;
                        }
                        break;

                    case ObservationType.Maximum:
                        if (value > _value)
                        {
                            _value = value;
                        }
                        break;
                }
                return Value;
            }
        }

        /// <summary>
        /// Resets the observed value back to "0", regardless of <see
        /// cref="ObservationType"/> setting.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _value = 0;
            }
        }
    }

    public enum ObservationType : byte
    {
        /// <summary>
        /// The "last observed" value will be returned from the counter.
        /// </summary>
        Last = 0,

        /// <summary>
        /// The "smallest value" observed will be returned from the counter.
        /// </summary>
        Minimum = 1,

        /// <summary>
        /// The "largest value" observed will be returned from the counter.
        /// </summary>
        Maximum = 2
    }
}
