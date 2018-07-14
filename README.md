# X4D Diagnostics
[![Build status](https://ci.appveyor.com/api/projects/status/d1nvjxg6gfjt3in3?svg=true)](https://ci.appveyor.com/project/wilson0x4d/diagnostics)

Shared interfaces, implementations and tools for an improved Diagnostic experience when writing **.NET Standard** libraries.

> NOTE: This is a work-in-progress, while there are some basic verification tests you may still find bugs. Further, as support within .NET Standard, .NET Core and .NET Framework so too may the behavior of this code.

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

<!-- /code_chunk_output -->


## Counters

the `X4D.Diagnostics.Counters` namespace exposes a lightweight framework for defining performance counters, categories of counters, and to easily access cached instances of those counters at run-time.

### Counter Integration

Consider the following code which increment the counters `InPerSec`, `ErrorsPerSec` and `OutPerSec` and `PendingTimeAverage`:

```csharp
    public sealed class Foo
    {
        private static readonly Performance = typeof(Foo).GetCounterCategory<FakeCounterCategory>();

        public async Task Bar()
        {
            var stopwatch = Stopwatch.StartNew();
            Performance.InPerSec.Increment();
            try
            {
                Console.WriteLine("Hello, World!");
            }
            finally
            {
                if (System.Runtime.InteropServices.Marshal.GetExceptionCode() != 0)
                {
                    Performance.ErrorsPerSec.Increment();
                }
                Performance.PendingTimeAverage.Increment(stopwatch);
                Performance.OutPerSec.Increment();
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

You may have noticed that the `Bar()` method above only incremented `InPerSec` and not `InTotal`, this is because `RatePerSecond` implementation will use `InTotal` as its numerator.  This has useful implications, for example there is no need to increment `PendingCounter` since it is driven by the `InTotal` and `OutTotal` counters, which are themselves driven by incrementing the `InperSec` and `OutPerSec` counters. This allows us to easy introduce counters which use existing counters as a basis, without needing to update any existing counter integration points (something that is usually relegated/deferred to a post-ingest process first for fear of breaking existing code.)

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

