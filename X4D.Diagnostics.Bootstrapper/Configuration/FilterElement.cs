using System.Configuration;

namespace X4D.Diagnostics.Boostrapper.Configuration
{
    internal sealed class FilterElement :
        ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false, IsKey = true)]
        public string Name
        {
            get => base["name"] as string;
            internal set => base["name"] = value;
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string TypeName
        {
            get => base["type"] as string;
            internal set => base["type"] = value;
        }

        [ConfigurationProperty("initializeData", IsRequired = true)]
        public string InitializeData
        {
            get => base["initializeData"] as string;
            internal set => base["initializeData"] = value;
        }
    }
}