using System.Collections.Generic;

namespace X4D.Diagnostics
{
    internal static class FibonacciHelper
    {
        /// <summary>
        /// Yield a finite fibonacci sequence as an <see cref="IEnumerable{long}"/>
        /// </summary>
        /// <param name="count">number of elements to yield.</param>
        internal static IEnumerable<long> ComputeTo(
            int count)
        {
            var fib = new[] { 0L, 1L };
            while (count-- >= 0)
            {
                yield return fib[0];
                var next = fib[0] + fib[1];
                fib[0] = fib[1];
                fib[1] = next;
            }
        }
    }
}
