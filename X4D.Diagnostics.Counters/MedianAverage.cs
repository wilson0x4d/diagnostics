using System.Collections.Generic;
using System.Linq;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates the Median Value of a Sorted Set.
    /// <para>
    /// This is considered a poor-performance class, it should not be used in
    /// production scenarios unless <see cref="Reset"/> is regularly called.
    /// </para>
    /// <para>
    /// If <see cref="Reset"/> is not called regularly then the performance
    /// of <see cref="Value"/> will slowly degrade as more memory and CPU is
    /// required to calculate a Median Value.
    /// </para>
    /// </summary>
    public class MedianAverage :
        ISupportsIncrement<long>,
        ISupportsReset
    {
        /// <summary>
        /// a Read/Write Lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// a Sorted Set used to calculate Median Value.
        /// </summary>
        private readonly IDictionary<long, long> _set;

        /// <summary>
        /// a Total Count of values stored to the Sorted Set (not to be
        /// confused with the total number of elements in the set.)
        /// </summary>
        private long _totalCount;

        /// <summary>
        /// Constructs a <see cref="MedianAverage"/> instance.
        /// </summary>
        public MedianAverage(
            object syncRoot = null)
        {
            _set = new SortedDictionary<long, long>();
            _lock = syncRoot ?? new object();
        }

        /// <summary>
        /// Gets a Median Average of all Values.
        /// </summary>
        public long Value
        {
            get
            {
                KeyValuePair<long, long>[] set;
                long totalCount;
                lock (_lock)
                {
                    set = _set.ToArray();
                    totalCount = _totalCount;
                }
                var midpoint = _totalCount / 2;
                var count = 0L;
                var result = 0L;
                for (int i = 0; i < set.Length; i++)
                {
                    var item = set[i];
                    result = item.Key;
                    count += item.Value;
                    if (count >= midpoint)
                    {
                        if (count == midpoint && midpoint * 2 == _totalCount)
                        {
                            return set[i].Key + ((set[i + 1].Key - set[i].Key) / 2);
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Adds the specified <paramref name="value"/> to the Sorted Set.
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
                _totalCount++;
                _set[value] = _set.TryGetValue(value, out long count)
                        ? count + 1
                        : 1;
                return _totalCount;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _totalCount = 0;
                _set.Clear();
            }
        }
    }
}
