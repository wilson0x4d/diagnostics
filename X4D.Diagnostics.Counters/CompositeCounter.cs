using System.Collections.Generic;
using System.Linq;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Represents a "Composite" counter, a composition of one or more
    /// counters where all Increment and Decrement operations are performed
    /// against on all counters.
    /// </summary>
    public sealed class CompositeCounter :
        ICounter<long>,
        ISupportsIncrement<long>,
        ISupportsDecrement<long>
    {
        /// <summary>
        /// an addcounter/removecounter lock.
        /// <para>
        /// thread-safety of calls into individual counters is the
        /// responsibility of each individual counter.
        /// </para>
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// The set of Assigned Counters.
        /// </summary>
        private ICounter<long>[] _counters;

        /// <summary>
        /// Constructs a <see cref="CompositeCounter"/> instance.
        /// </summary>
        /// <param name="counters"></param>
        public CompositeCounter(
            IEnumerable<ICounter<long>> counters = null,
            object syncRoot = null)
        {
            _counters = (counters != null)
                ? counters.ToArray()
                : new ICounter<long>[0];
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Returns the value of the First Counter in the set of Assigned Counters.
        /// <para>Order is determined chronologically.</para>
        /// </summary>
        public long Value
        {
            get
            {
                return _counters
                    ?.FirstOrDefault()
                    ?.Value
                    ?? 0L;
            }
        }

        /// <summary>
        /// Add a Counter to the set of Assigned Counters.
        /// <para>
        /// Re-adding a Counter will not create a duplicate entry but will
        /// instead place the Counter at the end of the set of Assigned Counters.
        /// </para>
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public CompositeCounter AddCounter(
            ICounter<long> counter)
        {
            lock (_lock)
            {
                _counters = _counters
                    .Where(e => e != counter)
                    .Concat(new[] { counter })
                    .ToArray();
            }
            return this;
        }

        /// <summary>
        /// Decrements all Assigned Counters by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Decrement(
            long value)
        {
            var counters = _counters;
            foreach (var item in counters)
            {
                if (item is ISupportsDecrement<long> supportsDecrement)
                {
                    supportsDecrement.Decrement(value);
                }
            }
            return counters[0]?.Value ?? 0L;
        }

        /// <summary>
        /// Increments all Assigned Counters by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Increment(
            long value)
        {
            var counters = _counters;
            foreach (var item in counters)
            {
                if (item is ISupportsIncrement<long> supportsIncrement)
                {
                    supportsIncrement.Increment(value);
                }
            }
            return counters[0]?.Value ?? 0L;
        }

        /// <summary>
        /// Remove a counter from the set of Assigned Counters.
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public CompositeCounter RemoveCounter(
            ICounter<long> counter)
        {
            if (_counters.Contains(counter))
                lock (_lock)
                    if (_counters.Contains(counter))
                    {
                        _counters = _counters
                            .Where(e => e != counter)
                            .ToArray();
                    }
            return this;
        }
    }
}
