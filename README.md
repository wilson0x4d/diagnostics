# X4D Diagnostics

Shared interfaces, implementations and tools for an improved Diagnostic experience when writing **.NET Standard** libraries.

> NOTE: This is a work-in-progress, while there are some basic verification tests you may still find bugs. Further, as support within .NET Standard, .NET Core and .NET Framework so too may the behavior of this code.

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
<!-- code_chunk_output -->

* [Counters](#counters)
	* [Data Collection](#data-collection)
* [Logging](#logging)
	* [Logging `Exception` objects](#logging-exception-objects)
	* [Logging `String` objects](#logging-string-objects)
	* [Logging `StringBuilder` objects](#logging-stringbuilder-objects)
	* [Logging `TextReader` objects](#logging-textreader-objects)

<!-- /code_chunk_output -->


## Counters

APIs to define counters, categories of counters, and to easily access cached instances of those counters.

Consider the following:

```csharp
    // observe some code using `FakeCounterCategory`
    typeof(T).GetCounterCategory<FakeCounterCategory>().Observe(() =>
    {
        // TODO: some code
    });
```

The `Observe()` method seen here is not part of the API, but it is convenient. We only define it on `FakeCounterCategory` for ease of use: 

```csharp
    public void Observe(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        InPerSec.Increment();
        try
        {
            action();
        }
        finally
        {
            if (System.Runtime.InteropServices.Marshal.GetExceptionCode() != 0)
            {
                ErrorsPerSec.Increment();
            }
            PendingTimeAverage.Increment(stopwatch);
            OutPerSec.Increment();
        }
    }
```


The `FakeCounterCategory` class defines a number of counters. Their allocation occurs in the counter category constructor. Notice how some counters incorporate or depend on others:

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

You may have noticed that the `Observe()` method had only incremented `InPerSec` and not `InTotal`, this is because `RatePerSecond` implementation will use `InTotal` as its numerator.  This has useful implications, for example there is no need to increment `PendingCounter` since it is driven by the `InTotal` and `OutTotal` counters, which are themselves driven by incrementing the `InperSec` and `OutPerSec` counters.

A good practice is to implement your counters so that counters which are interdependent share a common base name. This is not enforced, but convenient. In the above example you can see common base names such as "In", "Out" and "Errors", it's reasonable to assume that any "XxxPerSecond" counter will be incrementing a relevant "Xxx" base.

See Also:

* [Coded Test: CountersExtensions_Type_CanCreateCounterCategory](X4D.Diagnostics.Test\Counters\CountersExtensionsTests.cs)
* [Example Implementation: FakeCounterCategory](X4D.Diagnostics.Test\Counters\Fakes\FakeCounterCategory.cs)


### Data Collection

Every Counter Category has a `Monitor` field which allows consumers to add observers to the entire category. Observers can be added with differing observation intervals. Observers do not access Counters nor the Counter Category directly, instead they receive a snapshot of key/value pairs.

There are no out-of-box collectors at this time.


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

