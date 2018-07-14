using System;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Provides a common base for "Counter Categories" to build upon,
    /// providing a surface for integration with platform-specific services.
    /// <para>
    /// Subclassing ensures every category exposes a <see cref="Name"/> property.
    /// </para>
    /// <para>Subclassing ensures every category is reachable via <see cref="CounterCategoryFactory{TCounterCategory}.GetInstance(string)"/></para>
    /// <para>May be extended over time, or by platform-specific libraries.</para>
    /// </summary>
    /// <typeparam name="TCounterCategory"></typeparam>
    public abstract class CounterCategoryBase<TCounterCategory>
        where TCounterCategory : CounterCategoryBase<TCounterCategory>
    {
        /// <summary>
        /// The Monitor assigned to this Counter Category.
        /// </summary>
        public readonly CounterCategoryMonitor<TCounterCategory> Monitor;

        protected CounterCategoryBase(
            string name,
            object syncRoot = null)
        {
            var type = GetType();
            if (!(type.BaseType == typeof(CounterCategoryBase<TCounterCategory>)))
            {
                throw new NotSupportedException($"Type '{GetType().FullName}' does not directly inherit '{typeof(CounterCategoryBase<>).Name}'.");
            }
            if (!type.IsAssignableFrom(typeof(TCounterCategory)))
            {
                throw new NotSupportedException($"Type '{GetType().FullName}' specifies an incorrect type parameter '{typeof(TCounterCategory).Name}'.");
            }
            if (!type.IsSealed)
            {
                throw new NotSupportedException($"Type '{GetType().FullName}' must be a sealed type.");
            }
            Name = name;
            Monitor = new CounterCategoryMonitor<TCounterCategory>(this as TCounterCategory);
        }

        /// <summary>
        /// Gets the Instance Name of this Counter Category.
        /// <para>
        /// Instance Names should be unique within the scope of a single
        /// <typeparamref name="TCounterCategory"/> type.
        /// </para>
        /// <para>
        /// Instance Names can be shared between different <typeparamref name="TCounterCategory"/> types.
        /// </para>
        /// </summary>
        public string Name { get; private set; }
    }
}
