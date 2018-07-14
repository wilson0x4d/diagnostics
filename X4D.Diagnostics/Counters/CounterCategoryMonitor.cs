using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// Takes periodic snapshots of a <see
    /// cref="CounterCategoryBase{TCounterCategory}"/> instance and activates
    /// configured observers.
    /// <para>
    /// Every <see cref="CounterCategoryBase{TCounterCategory}"/> has a <see cref="CounterCategoryMonitor{TCounterCategory}"/>.
    /// </para>
    /// </summary>
    public sealed class CounterCategoryMonitor<TCounterCategory>
        where TCounterCategory : CounterCategoryBase<TCounterCategory>
    {
        /// <summary>
        /// a <see cref="WeakReference{TCounterCategory}"/> used by snapshot
        /// code to acquire a Counter Category instance.
        /// <para>
        /// because "Counter Category has a Counter Category Monitor" and we
        /// do not wish for circular reference.
        /// </para>
        /// </summary>
        private readonly WeakReference<TCounterCategory> _counterCategoryWeakRef;

        /// <summary>
        /// a Write Lock for AddObserver/RemoveObserver.
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// the Scheduled Observers.
        /// </summary>
        private ScheduledObserver[] _observers = new ScheduledObserver[0];

        /// <summary>
        /// the <see cref="Task"/> responsible for taking snapshots and activating Scheduled Observers.
        /// </summary>
        private Task _observationTask;

        /// <summary>
        /// the Counter Descriptors.
        /// </summary>
        private CounterDescriptor[] _counterDescriptors;

        /// <summary>
        /// the Minimum Interval for <see cref="_observationTask"/> to take a
        /// snapshot and attempt to activate observers.
        /// </summary>
        private TimeSpan _observerMinInterval;

        /// <summary>
        /// Constructs a <see
        /// cref="CounterCategoryMonitor{TCounterCategory}"/> instance for
        /// the specified <paramref name="counterCategory"/>.
        /// </summary>
        /// <param name="counterCategory"></param>
        internal CounterCategoryMonitor(
            TCounterCategory counterCategory)
        {
            if (typeof(TCounterCategory).BaseType != typeof(CounterCategoryBase<TCounterCategory>))
            {
                throw new NotSupportedException($"Type '{typeof(TCounterCategory).FullName}' does not directly inherit '{typeof(CounterCategoryBase<>).Name}'.");
            }
            _counterCategoryWeakRef = new WeakReference<TCounterCategory>(counterCategory);
            _counterDescriptors = typeof(TCounterCategory)
                .GetFields()
                .Where(e =>
                    e.Attributes.HasFlag(FieldAttributes.Public | FieldAttributes.InitOnly)
                    && e.FieldType.GetInterfaces().Any(iface => iface.GetGenericTypeDefinition() == typeof(ICounter<>)))
                .Select(fieldInfo =>
                {
                    return new CounterDescriptor
                    {
                        FieldInfo = fieldInfo,
                        ValueAccessor = fieldInfo
                            .FieldType
                            .GetProperty(nameof(ICounter<dynamic>.Value))
                            .GetMethod
                    };
                })
                .ToArray();
            if (_counterDescriptors.Length == 0)
            {
                throw new NotSupportedException($"Counter Category '{typeof(TCounterCategory).FullName}' does not expose any `public readonly` counters.");
            }
        }

        /// <summary>
        /// Add the specified <paramref name="action"/> (aka. Observer) to
        /// the Monitor.
        /// <para>Applies a default observation interval of 1 second.</para>
        /// </summary>
        /// <param name="observer"></param>
        public void AddObserver(
            Action<IDictionary<string, object>> action)
        {
            AddObserver(
                action,
                TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Add the specified <paramref name="action"/> (aka. Observer) to
        /// the Monitor using the specified <paramref name="interval"/>.
        /// <para>
        /// An observation <paramref name="interval"/> less than 100ms will
        /// be clamped to 100ms.
        /// </para>
        /// </summary>
        public void AddObserver(
            Action<IDictionary<string, object>> action,
            TimeSpan interval)
        {
            if (interval.TotalMilliseconds < 100)
            {
                interval = TimeSpan.FromMilliseconds(100);
            }
            lock (_lock)
            {
                var observerMinInterval = interval;
                _observers = _observers
                    .Where(e =>
                    {
                        if (e.Observe == action)
                        {
                            // remove any pre-existing entry for the observer
                            return false;
                        }
                        // recalculate `_observerMinInterval` for retained entries
                        if (observerMinInterval > e.Interval)
                        {
                            observerMinInterval = e.Interval;
                        }
                        return true;
                    })
                    .Concat(
                        new[] {
                            new ScheduledObserver
                            {
                                Interval = interval,
                                ScheduledFor = DateTime.UtcNow.Add(interval),
                                Observe = action
                            }
                        })
                    .ToArray();
                _observerMinInterval = observerMinInterval;
                if (_observationTask == null)
                {
                    _observationTask = StartObserving();
                }
            }
        }

        /// <summary>
        /// Remove the specified <paramref name="action"/> (aka. Observer)
        /// from the Monitor.
        /// </summary>
        /// <param name="action"></param>
        public void RemoveObserver(
            Action<IDictionary<string, object>> action)
        {
            lock (_lock)
            {
                var observerMinInterval = TimeSpan.MaxValue;
                _observers = _observers
                    .Where(e =>
                    {
                        if (e.Observe == action)
                        {
                            // remove any pre-existing entry for the observer
                            return false;
                        }
                        // recalculate `_observerMinInterval` for retained entries
                        if (observerMinInterval > e.Interval)
                        {
                            observerMinInterval = e.Interval;
                        }
                        return true;
                    })
                    .ToArray();
                if (observerMinInterval != TimeSpan.MaxValue)
                {
                    // only take a new min interval when it could be recalculated
                    _observerMinInterval = observerMinInterval;
                }
            }
        }

        /// <summary>
        /// Creates a snapshot of counter values.
        /// </summary>
        /// <returns>True if snapshot could be created, otherwise false.</returns>
        public bool TryGetSnapshot(out IDictionary<string, object> snapshot)
        {
            snapshot = default(IDictionary<string, object>);
            try
            {
                var counterCategory = default(TCounterCategory);
                snapshot = _counterDescriptors
                    .Select(counterDescriptor =>
                    {
                        if (counterDescriptor.Counter == null)
                        {
                            if (counterCategory == null && !_counterCategoryWeakRef.TryGetTarget(out counterCategory))
                            {
                                throw new Exception($"Counter Category instance no longer exists. Snapshot fails.");
                            }
                            counterDescriptor.Counter = counterDescriptor.FieldInfo.GetValue(counterCategory);
                        }
                        return new KeyValuePair<string, object>(
                            counterDescriptor.FieldInfo.Name,
                            counterDescriptor.ValueAccessor.Invoke(counterDescriptor.Counter, null));
                    })
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return snapshot != null;
        }

        /// <summary>
        /// A private method which spawns a Task responsible for observing
        /// Counter Snapshots using scheduled observers.
        /// </summary>
        /// <returns></returns>
        private async Task StartObserving()
        {
            while (_observers.Length > 0)
            {
                await Task.Delay(_observerMinInterval);
                var observers = _observers
                    .Where(observer => observer.ScheduledFor <= DateTime.UtcNow)
                    .ToArray();
                if (observers.Length > 0)
                {
                    if (TryGetSnapshot(out IDictionary<string, object> snapshot))
                    {
                        for (int i = 0; i < observers.Length; i++)
                        {
                            var observer = observers[i];
                            observer.ScheduledFor = DateTime.UtcNow.Add(observer.Interval);
                            try
                            {
                                observer.Observe(snapshot);
                            }
                            catch (Exception ex)
                            {
                                ex.Log();
                            }
                        }
                    }
                }
            }
            _observationTask = null;
        }

        /// <summary>
        /// A state object representing an observer to be activated during publish.
        /// <para>
        /// Any observer added to a Monitor is stored into a <see
        /// cref="ScheduledObserver"/> instance as part of scheduling.
        /// </para>
        /// </summary>
        private sealed class ScheduledObserver
        {
            /// <summary>
            /// The scheduling interval.
            /// </summary>
            public TimeSpan Interval;

            /// <summary>
            /// The next scheduled time.
            /// </summary>
            public DateTime ScheduledFor;

            /// <summary>
            /// The observer action to execute.
            /// </summary>
            public Action<IDictionary<string, object>> Observe;
        }

        /// <summary>
        /// A descriptor to make reflecting counter values more efficient.
        /// </summary>
        private sealed class CounterDescriptor
        {
            /// <summary>
            /// the Counter Category Counter field <see cref="FieldInfo"/>.
            /// </summary>
            public FieldInfo FieldInfo;

            /// <summary>
            /// the <see cref="ICounter{T}"/> instance.
            /// <para>remains null until initialized by snapshot code</para>
            /// </summary>
            public object Counter;

            /// <summary>
            /// the <see cref="ICounter{T}.Value"/> accessor <see cref="MethodInfo"/>.
            /// </summary>
            public MethodInfo ValueAccessor;
        }
    }
}
