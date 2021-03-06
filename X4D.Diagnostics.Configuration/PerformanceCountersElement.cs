﻿using System.Configuration;

namespace X4D.Diagnostics.Configuration
{
    internal sealed class PerformanceCountersElement :
        ConfigurationElement
    {
        [ConfigurationProperty("filemappingsize", DefaultValue = 524288)]
        public int FileMappingSize =>
            (int)base["filemappingsize"];
    }
}