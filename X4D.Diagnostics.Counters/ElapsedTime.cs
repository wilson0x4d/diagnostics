using System.Diagnostics;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Calculates "Elapsed Time", expressed as 64 bit "Ticks",
    /// "Milliseconds", "Seconds", "Minutes", "Hours" or "Days".
    /// </summary>
    public class ElapsedTime :
        ICounter<long>,
        ISupportsReset
    {
        private ElapsedTimeUnitType _unitType;

        private Stopwatch _stopwatch;

        /// <summary>
        /// Constructs an <see cref="ElapsedTime"/> instance with the
        /// specified <paramref name="unitType"/>.
        /// </summary>
        /// <param name="unitType"></param>
        public ElapsedTime(
            ElapsedTimeUnitType unitType = ElapsedTimeUnitType.Milliseconds)
        {
            _unitType = unitType;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Unit of Measure for <see cref="ElapsedTime"/>; ie. Ticks,
        /// Milliseconds, Seconds, etc.
        /// </summary>
        public enum ElapsedTimeUnitType : long
        {
            Ticks = 0,
            Milliseconds = 10000,
            Seconds = Milliseconds * 1000,
            Minutes = Seconds * 60,
            Hours = Minutes * 60,
            Days = Hours * 24
        }

        /// <summary>
        /// Gets the current Unit of Measure for <see cref="ElapsedTime"/>,
        /// expressed as <see cref="ElapsedTimeUnitType"/>.
        /// </summary>
        public ElapsedTimeUnitType UnitType => _unitType;

        /// <summary>
        /// Gets the elapsed time since counter creation, or last reset.
        /// </summary>
        public long Value
        {
            get
            {
                var elapsed = _stopwatch.Elapsed.Ticks;
                if (_unitType != ElapsedTimeUnitType.Ticks)
                {
                    elapsed /= (long)_unitType;
                }
                return elapsed;
            }
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }
}
