using System.Configuration;
using System.Linq;

namespace X4D.Diagnostics.Configuration
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

        protected override void PostDeserialize()
        {
            base.PostDeserialize();
            // NOTE: if not already being called be the bootstrapper, use the
            //       configuration being loaded as a default config at the
            //       point it is being deserialized.
            var stackTrace = new System.Diagnostics.StackTrace();
            if (!stackTrace.GetFrames().Any(stackFrame => stackFrame.GetMethod()?.DeclaringType?.FullName == typeof(SystemDiagnosticsBootstrapper).FullName))
            {
                SystemDiagnosticsBootstrapper.Configure(this);
            }
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
