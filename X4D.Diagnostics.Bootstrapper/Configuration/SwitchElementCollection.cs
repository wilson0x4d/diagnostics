using System.Configuration;

namespace X4D.Diagnostics.Boostrapper.Configuration
{
    [ConfigurationCollection(typeof(SwitchElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    internal class SwitchElementCollection :
        ConfigurationElementCollection
    {
        new public SwitchElement this[string name] =>
            BaseGet(name) as SwitchElement;

        // Methods
        protected override ConfigurationElement CreateNewElement() =>
            new SwitchElement();

        protected override object GetElementKey(ConfigurationElement element) =>
            (element as SwitchElement).Name;
    }
}