﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using X4D.Diagnostics.Configuration;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.Configuration
{
    /// <summary>
    /// Loads the `system.diagnostics` section and translates the
    /// configuration into run-time objects that support <see cref="LoggingExtensions"/>.
    /// </summary>
    public static class SystemDiagnosticsBootstrapper
    {
        /// <summary>
        /// When set, used in lieu of <see cref="ConfigurationManager"/> for access to `system.diagnostics` section.
        /// </summary>
        private static System.Configuration.Configuration _preferredConfiguration;

        /// <summary>
        /// When set, used in lieu of <see cref="ConfigurationManager"/> for access to `system.diagnostics` section.
        /// </summary>
        private static SystemDiagnosticsConfigurationSection _preferredConfigurationSection;

        private static IDictionary<string, Switch> _switches;
        private static IDictionary<string, TraceListener> _listeners;
        private static IDictionary<string, TraceSource> _sources;

        public static TraceSource GetConfiguredSource(string name)
        {
            var traceSource = default(TraceSource);
            if (_sources == null)
            {
                // attempt to auto-bootstrap the config
                Configure();
            }
            _sources?.TryGetValue(name, out traceSource);
            return traceSource;
        }

        /// <summary>
        /// Configure `System.Diagnostics` using <see cref="ConfigurationManager"/>.
        /// </summary>
        public static ConfigurationSection Configure()
        {
            SystemDiagnosticsConfigurationSection configurationSection;
            if (_preferredConfigurationSection != null)
            {
                configurationSection = _preferredConfigurationSection;
            }
            else if (_preferredConfiguration != null)
            {
                configurationSection = _preferredConfiguration.GetSection("system.diagnostics") as SystemDiagnosticsConfigurationSection;
            }
            else
            {
                configurationSection = ConfigurationManager.GetSection("system.diagnostics") as SystemDiagnosticsConfigurationSection;
            }            
            return Configure(configurationSection);
        }

        /// <summary>
        /// Configure `System.Diagnostics` using specified <paramref name="configuration"/>.
        /// <para>
        /// When called, we infer that caller prefers that <see
        /// cref="LoggingExtensions"/> uses the specifeid <paramref name="configuration"/>.
        /// </para>
        /// </summary>
        /// <param name="configuration"></param>
        public static ConfigurationSection Configure(
            System.Configuration.Configuration configuration)
        {
            _preferredConfiguration = configuration;
            var configurationSection = configuration.GetSection("system.diagnostics") as SystemDiagnosticsConfigurationSection;
            return Configure(configurationSection);
        }

        /// <summary>
        /// Configure `System.Diagnostics` using specified <paramref name="systemDiagnosticsConfigurationSection"/>.
        /// </summary>
        /// <param name="systemDiagnosticsConfigurationSection"></param>
        internal static ConfigurationSection Configure(
            SystemDiagnosticsConfigurationSection systemDiagnosticsConfigurationSection)
        {
            _preferredConfigurationSection = systemDiagnosticsConfigurationSection;
            if (systemDiagnosticsConfigurationSection != null)
            {
                _switches = InstantiateSwitches(systemDiagnosticsConfigurationSection);
                _listeners = InstantiateSharedListeners(systemDiagnosticsConfigurationSection);
                InitializeTraceTraceListeners(systemDiagnosticsConfigurationSection);
                _sources = InstantiateSources(systemDiagnosticsConfigurationSection, _switches, _listeners);
            }
            return systemDiagnosticsConfigurationSection;
        }

        /// <summary>
        /// Initialize the Trace Listeners assigned to <see cref="System.Diagnostics.Trace.Listeners"/>.
        /// </summary>
        /// <param name="systemDiagnosticsConfigurationSection"></param>
        private static void InitializeTraceTraceListeners(
            SystemDiagnosticsConfigurationSection systemDiagnosticsConfigurationSection)
        {
            // TODO: support `<clear/>`
            foreach (TraceListenerElement listenerElement in systemDiagnosticsConfigurationSection.Trace.Listeners)
            {
                if (_listeners.TryGetValue(listenerElement.Name, out TraceListener traceListener))
                {
                    Trace.Listeners.Remove(listenerElement.Name);
                    Trace.Listeners.Add(traceListener);
                }
            }
        }

        /// <summary>
        /// Constructs <see cref="TraceSource"/> instances for the current
        /// `system.diagnostics/trace` configuration.
        /// </summary>
        /// <param name="systemDiagnosticsConfigurationSection"></param>
        /// <param name="switches"></param>
        /// <param name="sharedListeners"></param>
        /// <returns></returns>
        private static IDictionary<string, TraceSource> InstantiateSources(
            SystemDiagnosticsConfigurationSection systemDiagnosticsConfigurationSection,
            IDictionary<string, Switch> switches,
            IDictionary<string, TraceListener> sharedListeners)
        {
            // TODO: `system.diagnostics/switches` are not properly evaluated
            //       at run-time, and cannot be used to control
            //       `system.diagnostics/sources`, although this bootstrapper
            //       does make an attempt to transpose switch values and most
            //       developers will not know the difference.
            var sources = new Dictionary<string, TraceSource>(StringComparer.OrdinalIgnoreCase);
            if (systemDiagnosticsConfigurationSection.Sources != null)
            {
                foreach (SourceElement sourceElement in systemDiagnosticsConfigurationSection.Sources)
                {
                    var traceSource = new TraceSource(sourceElement.Name);
                    var switchValue = !string.IsNullOrWhiteSpace(sourceElement.SwitchValue)
                                    ? sourceElement.SwitchValue
                                    : $"{SourceLevels.All}";
                    if (!string.IsNullOrWhiteSpace(sourceElement.SwitchType))
                    {
                        try
                        {
                            traceSource.Switch =
                                Activator.CreateInstance(
                                    Type.GetType(sourceElement.SwitchType),
                                    sourceElement.SwitchName ?? $"{sourceElement.Name}",
                                    switchValue)
                                as SourceSwitch;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to load SourceSwitch '{sourceElement.SwitchName ?? sourceElement.Name}': {sourceElement.SwitchType}", ex);
                        }
                    }
                    else if (switches.TryGetValue(sourceElement.SwitchName, out Switch @switch))
                    {
                        if (@switch is TraceSwitch traceSwitch)
                        {
                            traceSource.Switch = new SourceSwitch(
                                sourceElement.SwitchName,
                                Enum.Parse(typeof(SourceLevels), Convert.ToString(traceSwitch.Level)).ToString());
                        }
                        else if (@switch is BooleanSwitch booleanSwitch)
                        {
                            traceSource.Switch = new SourceSwitch(
                                sourceElement.SwitchName,
                                booleanSwitch.Enabled
                                    ? $"{SourceLevels.All}"
                                    : $"{SourceLevels.Off}");
                        }
                        else
                        {
                            // TODO: need to look through reference source
                            //       and understand how switch configuration
                            //       is done in net45 -- this behavior here
                            //       is actually incorrect (and the behavior
                            //       above is also not entirely proper, but
                            //       does allow the use of switches as most
                            //       developers would expect them to work via
                            //       the config file.)
                            traceSource.Switch = new SourceSwitch(
                                sourceElement.SwitchName,
                                switchValue);
                        }
                    }
                    else
                    {
                        traceSource.Switch.Level = Enum.TryParse(switchValue, out SourceLevels sourceLevel)
                            ? sourceLevel
                            : SourceLevels.All;
                    }

                    foreach (TraceListenerElement traceListenerElement in sourceElement.Listeners)
                    {
                        traceSource.Listeners.Remove(traceListenerElement.Name);
                        if (string.IsNullOrWhiteSpace(traceListenerElement.TypeName))
                        {
                            if (sharedListeners.TryGetValue(traceListenerElement.Name, out TraceListener traceListener))
                            {
                                traceSource.Listeners.Add(traceListener);
                            }
                            else
                            {
                                throw new Exception(
                                    $"Source '{sourceElement.Name}' references shared listener '{traceListenerElement.Name}' which does not exist in `<sharedListeners/>`.");
                            }
                        }
                        else
                        {
                            traceSource.Listeners.Add(
                                InstantiateTraceListener(traceListenerElement));
                        }
                    }
                    sources.Add(traceSource.Name, traceSource);
                }
            }
            return sources;
        }

        /// <summary>
        /// Constructs a <see cref="TraceListener"/> instance for the
        /// specified <paramref name="traceListenerElement"/>.
        /// </summary>
        /// <param name="traceListenerElement"></param>
        /// <returns></returns>
        private static TraceListener InstantiateTraceListener(
            TraceListenerElement traceListenerElement)
        {
            TraceListener traceListener;
            try
            {
                var traceListenerType = Type.GetType(traceListenerElement.TypeName);
                traceListener = Activator.CreateInstance(traceListenerType) as TraceListener;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load TraceListener '{traceListenerElement?.Name}': {traceListenerElement?.TypeName}", ex);
            }
            traceListener.Name = traceListenerElement.Name;
            traceListener.TraceOutputOptions = traceListenerElement.TraceOutputOptions;
            if (traceListenerElement.Filter != null)
            {
                var filterType = Type.GetType(traceListenerElement.Filter.TypeName);
                if (filterType != null)
                {
                    if (filterType == typeof(EventTypeFilter))
                    {
                        if (!Enum.TryParse(traceListenerElement.Filter.InitializeData, out SourceLevels sourceLevels))
                        {
                            sourceLevels = SourceLevels.All;
                        }
                        traceListener.Filter = new EventTypeFilter(sourceLevels);
                    }
                    else if (filterType == typeof(SourceFilter))
                    {
                        traceListener.Filter = new SourceFilter(traceListenerElement.Filter.InitializeData);
                    }
                    else
                    {
                        try
                        {
                            traceListener.Filter = string.IsNullOrWhiteSpace(traceListenerElement.Filter.InitializeData)
                                ? Activator.CreateInstance(filterType) as TraceFilter
                                : Activator.CreateInstance(filterType, traceListenerElement.Filter.InitializeData) as TraceFilter;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to load TraceFilter '{traceListenerElement?.Name}': {traceListenerElement?.TypeName}", ex);
                        }
                    }
                }
            }

            return traceListener;
        }

        /// <summary>
        /// Constructs shared <see cref="TraceListener"/> instances for the
        /// current `system.diagnostics/sharedListeners` configuration.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        private static IDictionary<string, TraceListener> InstantiateSharedListeners(
            SystemDiagnosticsConfigurationSection configurationSection)
        {
            var sharedListeners = new Dictionary<string, TraceListener>(StringComparer.OrdinalIgnoreCase);
            if (configurationSection.SharedListeners != null)
            {
                foreach (TraceListenerElement traceListenerElement in configurationSection.SharedListeners)
                {
                    try
                    {
                        var traceListener = InstantiateTraceListener(traceListenerElement);
                        sharedListeners.Add(traceListener.Name, traceListener);
                    }
                    catch (Exception)
                    {
                        // NOP: failure to instanciate a trace listener
                        //      should not result in a log line.
                    }
                }
            }

            return sharedListeners;
        }

        /// <summary>
        /// Constructs <see cref="Switch"/> instances for the current
        /// `system.diagnostics/switches` configuration.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        private static IDictionary<string, Switch> InstantiateSwitches(
            SystemDiagnosticsConfigurationSection configurationSection)
        {
            var switches = new Dictionary<string, Switch>(StringComparer.OrdinalIgnoreCase);
            if (configurationSection.Switches != null)
            {
                foreach (SwitchElement switchElement in configurationSection.Switches)
                {
                    // multiple rules dictate switch type
                    var @switch = default(Switch);
                    if (bool.TryParse(switchElement.Value, out bool boolValue))
                    {
                        // if value is a `bool`, use `BooleanSwitch`
                        @switch = new BooleanSwitch(switchElement.Name, "", switchElement.Value);
                    }
                    else if (Enum.TryParse(switchElement.Value, out TraceLevel traceLevel))
                    {
                        // if value is a `TraceLevel`, use `TraceSwitch`
                        @switch = new TraceSwitch(switchElement.Name, "", switchElement.Value);
                    }
                    else
                    {
                        throw new Exception(
                            $"Unsupported Value '{switchElement.Value}' for Switch '{switchElement.Name}'.");
                    }
                    switches.Add(switchElement.Name, @switch);
                }
            }
            return switches;
        }
    }
}
