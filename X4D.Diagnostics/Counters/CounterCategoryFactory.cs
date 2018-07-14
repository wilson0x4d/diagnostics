using System;
using System.Collections.Generic;
using System.Linq;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// A <typeparamref name="TCounterCategory"/> Factory which only
    /// exists to encapsulate instancing logic.
    /// </summary>
    public static class CounterCategoryFactory<TCounterCategory>
        where TCounterCategory : CounterCategoryBase<TCounterCategory>
    {
        private static IDictionary<string, WeakReference<TCounterCategory>> _instances =
            new Dictionary<string, WeakReference<TCounterCategory>>(StringComparer.OrdinalIgnoreCase);

        public static TCounterCategory GetInstance(
            Type type)
        {
            return GetInstance(
                GetInstanceNameForType(
                    type));
        }

        public static TCounterCategory GetInstance(
            string name)
        {
            if (!_instances.TryGetValue(name, out WeakReference<TCounterCategory> reference)
                || !reference.TryGetTarget(out TCounterCategory instance))
            {
                lock (_instances)
                {
                    if (!_instances.TryGetValue(name, out reference)
                        || !reference.TryGetTarget(out instance))
                    {
                        instance = Activator.CreateInstance(typeof(TCounterCategory), name) as TCounterCategory;
                        _instances[name] = new WeakReference<TCounterCategory>(instance);
                    }
                }
            }
            return instance;
        }

        private static string GetInstanceNameForType(
            Type type,
            bool useFullName = false)
        {
            if (useFullName)
            {
                return type.FullName;
            }
            else
            {
                var parts = type.FullName.Split('.', '+');
                if (parts.Length > 2)
                {
                    parts = parts
                        .Skip(parts.Length - 2)
                        .Take(2)
                        .ToArray();
                }
                return string.Join(".", parts);
            }
        }
    }
}
