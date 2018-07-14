using System.Configuration;

namespace X4D.Diagnostics.Configuration
{
    internal class SwitchElement :
        ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name =>
            base["name"] as string;

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value =>
            base["value"] as string;
    }
}