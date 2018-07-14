using System.Configuration;

namespace X4D.Diagnostics.Boostrapper.Configuration
{
    /// <summary>
    /// A stand-in ` <system.Diagnostics/>` configuration section
    /// implementation that should provide the same level of support for
    /// "Tracing" factilities as the .NET Framework variant.
    /// </summary>
    internal sealed class SystemDiagnosticsConfigurationSection :
        ConfigurationSection
    {
        public SystemDiagnosticsConfigurationSection()
        {
            base["assert"] = new AssertElement();
            base["performanceCounters"] = new PerformanceCountersElement();
            base["sources"] = new SourceElementCollection();
            base["sharedListeners"] = new TraceListenerElementCollection(true);
            base["switches"] = new SwitchElementCollection();
            base["trace"] = new TraceElement();
        }

        [ConfigurationProperty("assert")]
        public AssertElement Assert =>
            base["assert"] as AssertElement;

        [ConfigurationProperty("performanceCounters")]
        public PerformanceCountersElement PerfCounters =>
            base["performanceCounters"] as PerformanceCountersElement;

        [ConfigurationProperty("sources")]
        public SourceElementCollection Sources =>
            base["sources"] as SourceElementCollection;

        [ConfigurationProperty("sharedListeners")]
        public TraceListenerElementCollection SharedListeners =>
            base["sharedListeners"] as TraceListenerElementCollection;

        [ConfigurationProperty("switches")]
        public SwitchElementCollection Switches =>
            base["switches"] as SwitchElementCollection;

        [ConfigurationProperty("trace")]
        public TraceElement Trace =>
            base["trace"] as TraceElement;
    }
}
