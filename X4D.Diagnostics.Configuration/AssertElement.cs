using System.Configuration;

namespace X4D.Diagnostics.Configuration
{
    internal sealed class AssertElement :
        ConfigurationElement
    {
        [ConfigurationProperty("assertuienabled", DefaultValue = true)]
        public bool AssertUIEnabled =>
            (bool)base["assertuienabled"];

        [ConfigurationProperty("logfilename", DefaultValue = "")]
        public string LogFileName =>
            base["logfilename"] as string;
    }
}
