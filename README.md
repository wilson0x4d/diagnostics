# X4D Diagnostics
[![Build status](https://ci.appveyor.com/api/projects/status/d1nvjxg6gfjt3in3?svg=true)](https://ci.appveyor.com/project/wilson0x4d/diagnostics) ![netstandard](https://img.shields.io/badge/.net-standard-blue.svg) ![netframework](https://img.shields.io/badge/.net-framework-blue.svg) [![NuGet](https://img.shields.io/nuget/v/X4D.Diagnostics.svg)](https://www.nuget.org/packages/X4D.Diagnostics) [![Downloads](https://img.shields.io/nuget/dt/X4D.Diagnostics.svg)](https://www.nuget.org/api/v2/package/X4D.Diagnostics/)

Shared interfaces, implementations and tools for an improved Diagnostic experience when writing **.NET Standard** libraries.

> NOTE: This is a work-in-progress, while there are some basic verification tests you may still find bugs. Further, as support within .NET Standard, .NET Core and .NET Framework changes so too may the behavior of this code.

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
<!-- code_chunk_output -->

* [Counters](#counters)
	* [Counter Integration](#counter-integration)
	* [Counter Monitoring](#counter-monitoring)
	* [Counter Definition](#counter-definition)
* [Logging](#logging)
	* [Logging `Exception` objects](#logging-exception-objects)
	* [Logging `String` objects](#logging-string-objects)
	* [Logging `StringBuilder` objects](#logging-stringbuilder-objects)
	* [Logging `TextReader` objects](#logging-textreader-objects)
* [Custom Trace Listeners](#custom-trace-listeners)
	* [Listener: `ConsoleTraceListener`](#listener-consoletracelistener)
	* [Listener: `UdpTraceListener`](#listener-udptracelistener)
* [Bootstrapping `<system.diagnostics/>` in .NET Core](#bootstrapping-systemdiagnostics-in-net-core)

<!-- /code_chunk_output -->


## Counters

the `X4D.Diagnostics.Counters` namespace exposes a lightweight framework for defining performance counters, categories of counters, and to easily access cached instances of those counters at run-time. 


### Counter Integration

Consider the following code which increments the counters `InPerSec`, `ErrorsPerSec`, `OutPerSec` and `PendingTimeAverage`:

```csharp
    public sealed class Foo
    {
        private readonly FakeCounterCategory _performance = typeof(Foo).GetCounterCategory<FakeCounterCategory>();

        public async Task Bar()
        {
            var stopwatch = Stopwatch.StartNew();
            _performance.InPerSec.Increment();
            try
            {
                Console.WriteLine("Hello, World!");
            }
            finally
            {
                if (System.Runtime.InteropServices.Marshal.GetExceptionCode() != 0)
                {
                    _performance.ErrorsPerSec.Increment();
                }
                _performance.PendingTimeAverage.Increment(stopwatch);
                _performance.OutPerSec.Increment();
            }
        }
    }
```

### Counter Monitoring

Every Counter Category has a `Monitor` field which allows consumers to add observers to the entire category. Observers can be added with differing observation intervals. Observers do not access Counters nor the Counter Category directly, instead they receive a snapshot of key/value pairs.

There are no out-of-box collectors at this time, we would like to see third party integrations in the wild before attemmpting to provide any default implementations as part of the framework.

In this example we see that a reference to the `Foo` instance from above is not required, but we do require Type Identity (in the form of a generic type param) to gain access to the monitor:

```csharp
    // add an observer to the default `FakeCounterCategory` for `Foo`
    var performance = typeof(Foo).GetCounterCategory<FakeCounterCategory>();
    var perfmon = performance.Monitor;
    perfmon.AddObserver(
        (snapshot) =>
        {
            // log each snapshot, formatted as JSON
            var json = JsonConvert.SerializeObject(snapshot);
            json.Log(); // this can then be delivered to Splunk, Graylog2, syslog, etc via config
        },
        TimeSpan.FromMilliseconds(500));
```

> NOTE: Currently there is no platform-specific integration (ie. you cannot view these counts in `perfmon` just yet.) However, this code has been modelled in a way that it will fit within the framework of existing platform-specific instrumentation/telemetry APIs. For this reason the concrete implementations present today are distributed separate from the interfaces/etc. This is intentional, and done to ensure that platform-specific implementations can be distributed later without interfering with existing code.

### Counter Definition

The `FakeCounterCategory` class used above defines a number of counters. Their allocation occurs in the counter category constructor. Notice how some counters incorporate or depend on others:

```csharp
    public FakeCounterCategory(string name)
        : base(name)
    {
        InTotal = new SumTotal();
        InPerSec = new RatePerSecond(InTotal);
        OutTotal = new SumTotal();
        OutPerSec = new RatePerSecond(OutTotal);
        PendingCount = new Delta(
            InTotal,
            OutTotal);
        PendingTimeAverage = new MovingAverage();
        ErrorsTotal = new SumTotal();
        ErrorsPerSec = new RatePerSecond(ErrorsTotal);
        ErrorRatio = new MeanAverage(
            ErrorsTotal,
            InTotal);
    }
```

You may notice that `Bar()` above only incremented `InPerSec` and not `InTotal`, this is because `RatePerSecond` implementation will use `InTotal` as its numerator.  This has useful implications, for example there is no need to increment `PendingCounter` since it is driven by the `InTotal` and `OutTotal` counters, which are themselves driven by incrementing the `InperSec` and `OutPerSec` counters. This allows us to easy introduce counters which use existing counters as a basis, without needing to update any existing counter integration points (something that is usually relegated/deferred to a post-ingest process first for fear of breaking existing code.)

A good practice is to implement your counters so that counters which are interdependent share a common base name. This is not enforced, but convenient. In the above example you can see common base names such as "In", "Out" and "Errors", it's reasonable to assume that any "XxxPerSecond" counter will be incrementing a relevant "Xxx" base.

See Also:

* [The `X4D.Diagnostics.Counters`-specific README](https://github.com/wilson0x4d/diagnostics/blob/master/X4D.Diagnostics.Counters/README.md)
* [Coded Test: CountersExtensions_GetInstance_BasicVerification](https://github.com/wilson0x4d/diagnostics/blob/master/X4D.Diagnostics.Test/Counters/CountersExtensionsTests.cs)
* [Example Implementation: FakeCounterCategory](https://github.com/wilson0x4d/diagnostics/blob/master/X4D.Diagnostics.Test/Counters/Fakes/FakeCounterCategory.cs)


## Logging

There are many well-rounded, battle-tested logging frameworks, and there are many third-party integrations for those frameworks.

This is not another logging framework, instead you will find a set of extension methods that leverage the in-built logging facilities already present in .NET via `System.Diagnostics`. This does not preclude the use of a third-party framework.

### Logging `Exception` objects

You can log exceptions and expect a relatively complete output, compacted to a single line (making friendlier toward tail tools, and simpler to ingest/index for misc logging infrastructure/solutions.)

Input:

```csharp
    try
    {
        throw new FakeException("Hello, World!");
    }
    catch (Exception ex)
    {
        ex.Log();
    }
```

Output:

```text
Error: 1 : { Type = X4D.Diagnostics.Fakes.FakeException, Message = Hello, World!, StackTrace = ==(X4D.Diagnostics.Fakes.FakeException|Hello, World!)==\r\n   at X4D.Diagnostics.Logging.LoggingExtensionsTests.LoggingExtensions_Exception_CanLog() in Z:\wilson0x4d\diagnostics\X4D.Diagnostics.Test\Logging\LoggingExtensionsTests.cs:line 38\r\n, Data =  }
```

> NOTE: The actual format of the data logged, including any line prefix/suffix is entirely determine by the `TraceListener` used. In this case, this is the output produced by the `DefaultTraceListener` (ie. the default.) Thus, using a more advanced `TraceListener` (perhaps one provided by your "logging framework", "logging infrastructure", or something custom) should yield better formatting (for example JSON/XML, delivered to Graylog/Splunk over UDP.)


### Logging `String` objects

Input:

```csharp
    $"Hello, World!".Log();
```

Output:

```text
Information: 2 : Hello, World!
```


### Logging `StringBuilder` objects

Input:

```csharp
    var expectedMessage = Guid.NewGuid().ToString();
    var stringBuilder = new StringBuilder(expectedMessage);
    stringBuilder.Log();
```

Output:

```text
Information: 3 : 9cb1d5fe-0c67-44b8-a441-aaf6031a79f4
```


### Logging `TextReader` objects

Input:

```csharp
    var expectedMessage = Guid.NewGuid().ToString();
    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);
    using (var reader = new StreamReader(stream))
    {
        writer.Write(expectedMessage);
        writer.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        reader.Log();
    }
```

Output:

```text
Information: 4 : 4e20b9c7-954b-4a2a-aac5-ee006d0810be
```


## Custom Trace Listeners

Distributed in a standalone package [X4D.Diagnostics.TraceListeners](https://www.nuget.org/packages/X4D.Diagnostics.TraceListeners) is a small, lightweight set of `TraceListener` implementations valid for use from **.NET Standard**, **.NET Core** and **.NET Framework**.


### Listener: `ConsoleTraceListener`

The implmentation provided is similar to that of .NET Framework, and does not provide any configuration options. It has been added because there is no default implementation in .NET Core / .NET Standard as everyone expects.

You can add it to your diagnostics config like so:

```xml
    <add name="ConsoleLog"
         type="X4D.Diagnostics.TraceListeners.ConsoleTraceListener,X4D.Diagnostics.TraceListeners" />
```


### Listener: `UdpTraceListener`

This UDP trace listener currently only supports a JSON payload (minimally constructed), and utilizes `UdpClient` internally.

The intended purpose is to allow the delivery of trace events to a log server (Splunk, graylog2, logstash, etc.) -- a fairly common practice.

```xml
    <add name="UdpLog"
         type="X4D.Diagnostics.TraceListeners.UdpTraceListener,X4D.Diagnostics.TraceListeners"
         initializeData="localhost:514"/>
```

Take note that the UDP Host Name and Port Number can be customized using `initializeData`, in the example above you see the default config if no value is specified (allowing for a local log ingest by default, using a relatively common default port number.)

The resulting UDP messages contain a payload with the following structure:

```json
{
    "ts" : "2018-07-16T11:30:04.1553089Z",
    "level" : "Information",
    "message" : "a543baf1-097d-45fb-ac0f-feec44463058",
    "source" : "My.Program",
    "id" : 5,
    "host" : "DESKTOP-XYZ123",
    "pid" : 18593,
    "tid" : 1977
}
```

When delivered via the UDP the whitepsace shown above is NOT present, instead, all content appears condensed and on a single line.


## Bootstrapping `<system.diagnostics/>` in .NET Core

You will notice that under .NET Core your `<system.diagnostics/>` config section is NOT automatically loaded, and none of the behavior you might expect is present.

Distributed in a standalone package [X4D.Diagnostics.Configuration](https://www.nuget.org/packages/X4D.Diagnostics.Configuration) provides a bootstrapper to workaround this problem.

This bootstrapper is automatically activated the first time you use any of the `Log()` extension methods shown above.

You can manually bootstrap (for whatever reason) by including the following code in your program:

```csharp
    X4D.Diagnostics.Configuration.SystemDiagnosticsBootstrapper.Configure();
```

This can be useful if you're not using any of the extension methods, or if you need to control order of initialization vs. other components.
