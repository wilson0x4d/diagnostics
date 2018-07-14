using System.Diagnostics;

namespace X4D.Diagnostics.Fakes
{
    public sealed class FakeSourceSwitch :
        SourceSwitch
    {
        public FakeSourceSwitch(string name) 
            : base(name)
        {
        }

        public FakeSourceSwitch(string displayName, string defaultSwitchValue) 
            : base(displayName, defaultSwitchValue)
        {
        }
    }
}
