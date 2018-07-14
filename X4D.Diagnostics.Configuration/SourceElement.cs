using System.Configuration;

namespace X4D.Diagnostics.Configuration
{
    internal sealed class SourceElement :
        ConfigurationElement
    {
        public SourceElement()
        {
            base["listeners"] = new TraceListenerElementCollection(true);
        }

        [ConfigurationProperty("listeners")]
        public TraceListenerElementCollection Listeners =>
            (base["listeners"] as TraceListenerElementCollection);

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "")]
        public string Name =>
            base["name"] as string;

        [ConfigurationProperty("switchName")]
        public string SwitchName =>
            base["switchName"] as string;

        [ConfigurationProperty("switchType")]
        public string SwitchType =>
            base["switchType"] as string;

        [ConfigurationProperty("switchValue")]
        public string SwitchValue =>
            base["switchValue"] as string;
    }
}