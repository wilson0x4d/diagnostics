using System;
using System.Configuration;
using System.Diagnostics;

namespace X4D.Diagnostics.Configuration
{
    internal sealed class TraceListenerElement :
        ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false, IsKey = true)]
        public string Name
        {
            get => base["name"] as string;
            internal set => base["name"] = value;
        }

        [ConfigurationProperty("type")]
        public string TypeName
        {
            get => base["type"] as string;
            internal set => base["type"] = value;
        }

        [ConfigurationProperty("filter", IsRequired = false)]
        public FilterElement Filter
        {
            get =>
                base["filter"] as FilterElement;
            internal set =>
                base["filter"] = value;
        }

        [ConfigurationProperty("traceOutputOptions", IsRequired = false)]
        public TraceOptions TraceOutputOptions
        {
            get =>
                (TraceOptions)(base["traceOutputOptions"] ?? TraceOptions.None);
            internal set =>
                base["traceOutputOptions"] = value;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Properties.Add(
                new ConfigurationProperty(name, typeof(string)));
            base[name] = value;
            return true;
        }
    }
}
