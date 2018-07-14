using System.Configuration;

namespace X4D.Diagnostics.Boostrapper.Configuration
{
    [ConfigurationCollection(typeof(SourceElement), AddItemName = "source", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal sealed class SourceElementCollection :
        ConfigurationElementCollection
    {
        public SourceElementCollection()
        {
        }

        protected override string ElementName =>
            "source";

        protected override ConfigurationElement CreateNewElement()
        {
            return new SourceElement();
        }

        protected override object GetElementKey(ConfigurationElement element) =>
            (element as SourceElement).Name;
    }
}
