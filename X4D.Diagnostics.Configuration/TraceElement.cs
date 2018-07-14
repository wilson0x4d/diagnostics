using System.Configuration;

namespace X4D.Diagnostics.Configuration
{
    internal sealed class TraceElement :
        ConfigurationElement
    {
        public TraceElement()
        {
            base["listeners"] = new TraceListenerElementCollection(true);
        }

        [ConfigurationProperty("autoflush", DefaultValue = false)]
        public bool AutoFlush =>
            (bool)base["autoflush"];

        [ConfigurationProperty("indentsize", DefaultValue = 4)]
        public int IndentSize =>
            (int)base["indentsize"];

        [ConfigurationProperty("listeners")]
        public TraceListenerElementCollection Listeners =>
            base["listeners"] as TraceListenerElementCollection;

        [ConfigurationProperty("useGlobalLock", DefaultValue = true)]
        public bool UseGlobalLock =>
            (bool)this["useGlobalLock"];

    }
}