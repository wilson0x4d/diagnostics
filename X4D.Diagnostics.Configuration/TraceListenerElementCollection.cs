using System;
using System.Configuration;
using System.Diagnostics;

namespace X4D.Diagnostics.Configuration
{
    [ConfigurationCollection(typeof(TraceListenerElement))]
    internal sealed class TraceListenerElementCollection :
        ConfigurationElementCollection
    {
        public TraceListenerElementCollection()
            : this(true)
        { }

        public TraceListenerElementCollection(
            bool initializeDefault)
        {
            if (initializeDefault)
            {
                InitializeDefault();
            }
        }

        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        protected override ConfigurationElement CreateNewElement() =>
            new TraceListenerElement();

        protected override object GetElementKey(ConfigurationElement element) =>
            (element as TraceListenerElement).Name;

        protected override void InitializeDefault()
        {
            BaseAdd(
                new TraceListenerElement
                {
                    Name = "Default",
                    TypeName = typeof(DefaultTraceListener).AssemblyQualifiedName
                });
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (element is TraceListenerElement traceListenerElement)
            {
                BaseAdd(
                    traceListenerElement,
                    traceListenerElement.Name.Equals("Default", StringComparison.OrdinalIgnoreCase)
                    && typeof(DefaultTraceListener).Equals(Type.GetType(traceListenerElement.TypeName)));
            }
            else
            {
                throw new ArgumentException(
                    $"Element must be subclass of {nameof(TraceListenerElement)}",
                    nameof(element));
            }
        }
    }
}
