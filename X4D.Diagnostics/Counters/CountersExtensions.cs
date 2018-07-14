using System;
using System.Diagnostics;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// General purpose <see cref="X4D.Diagnostics.Counters"/> extension methods.
    /// </summary>
    public static class CountersExtensions
    {
        /// <summary>
        /// Get a <typeparamref name="TCounterCategory"/> instance for the <paramref name="type"/>.
        /// <para>
        /// Internally uses <see cref="CounterCategoryBase{TCounterCategory}.GetInstance(Type)"/>.</para>
        /// </summary>
        /// <typeparam name="TCounterCategory">implements <see cref="CounterCategoryBase{TCategory}"/></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TCounterCategory GetCounterCategory<TCounterCategory>(
            this Type type)
            where TCounterCategory : CounterCategoryBase<TCounterCategory>
        {
            return CounterCategoryFactory<TCounterCategory>.GetInstance(type);
        }
    }

    /// <summary>
    /// <see cref="ISupportsDecrement{T}"/> extension methods.
    /// </summary>
    public static class ISupportsDecrementExtensions
    {
        /// <summary>
        /// Performs a decrement operation of 1 unit.
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static long Decrement(
            this ISupportsDecrement<long> counter)
        {
            return counter.Decrement(1L);
        }

        /// <summary>
        /// Performs a decrement operation of <see cref="Stopwatch.ElapsedMilliseconds"/>.
        /// <para>
        /// Handy when collecting time deltas.
        /// </para>
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        public static long Decrement(
            this ISupportsDecrement<long> counter,
            Stopwatch stopwatch)
        {
            return counter.Decrement(stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// <see cref="ISupportsIncrement{T}"/> extension methods.
    /// </summary>
    public static class ISupportsIncrementExtensions
    {
        /// <summary>
        /// Performs an increment operation of 1 unit.
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static long Increment(
            this ISupportsIncrement<long> counter)
        {
            return counter.Increment(1L);
        }

        /// <summary>
        /// Performs an increment operation of <see cref="Stopwatch.ElapsedMilliseconds"/>.
        /// <para>
        /// Handy when collecting execution times.
        /// </para>
        /// </summary>
        public static long Increment(
            this ISupportsIncrement<long> counter,
            Stopwatch stopwatch)
        {
            return counter.Increment(stopwatch.ElapsedMilliseconds);
        }
    }
}
