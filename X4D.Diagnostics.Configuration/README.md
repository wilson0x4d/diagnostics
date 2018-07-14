# `X4D.Diagnostics.Configuration`

This package exists to solve problems with loading the `<system.diagnostics/>` config section under **.NET Core**.

This is only intended to be used from a .NET Core application, attempts to use this from a .NET Framework application will not provide the same results. In a .NET Framework application you will want to use the default framework-supplied functionality.

Eventually, ideally, this sort of "polyfill" will not be necessary.


### Configuration and Usage

First, you will want to edit your configuration file (app.config, web.config) and add/remove the `system.diagnostics` section.

```xml
<configuration>
    <configSections>
        <section name="system.diagnostics" 
                 type="X4D.Diagnostics.Configuration.SystemDiagnosticsConfigurationSection,X4D.Diagnostics.Configuration" />
```

You will also want to include a typical `<system.diagnostics>` config section, the unit test project has an example, and there are many great examples on MSDN.

Normally, configuration is all that is required. However, if you have a scenario where the configuration is not bootstrapping as expected you can attempt to manually load the config section:

```csharp
    var configuration = ConfigurationManager.OpenMappedExeConfiguration(
        new ExeConfigurationFileMap
        {
            ExeConfigFilename = $"{Assembly.GetExecutingAssembly().Location}.config"
        },
        ConfigurationUserLevel.None);
    var circularFile = configuration.GetSection("system.diagnostics");
```

Hope this helps, if you would like to see something changed please submit an issue on github.
