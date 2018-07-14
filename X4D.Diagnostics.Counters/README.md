# Project: `X4D.Diagnostics.Counters`

Exposes cross-platform Performance Counters that are valid for use from .NET Standard assemblies over all supported platforms.


<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->
<!-- code_chunk_output -->

* [Counters](#counters)
* [Categories](#categories)
	* [Example: Queue Statistics Category](#example-queue-statistics-category)
	* [Why does `CounterCategoryBase` have a type parameter?](#why-does-countercategorybase-have-a-type-parameter)
* [Base Counters](#base-counters)

<!-- /code_chunk_output -->


## Counters

A **Counter** provides a scalar result given an input series, such as a Total, Rate or Average.

| Counter | |
|-|-|
| `Delta` | Calculates the Difference between two counters referred to as the `Minuend` and the `Subtrahend`. |
| `ElapsedTime` | Calculates "Elapsed Time", expressed in 64 bit Ticks, Milliseconds, Seconds, Minutes, Hours, or Days. |
| `MeanAverage` | Calculates the Mean Average of a `Numerator` and `Denominator`. |
| `MedianAverage` | Calculates the Median Value of a Sorted Set. |
| `MovingAverage` | Calculates a Mean Average from a Finite Set, where new values cause older values to evict from the set (FIFO.) |
| `RatePerSecond` | Provides a rate of values per second over the period specified. |
| `SumTotal` | Tallies counts to produce a sum number of items." |

## Categories

A **Category** is a small collection of named counter instances, typically grouped into a category to provide metrics over a wide class of componenets. Multiple categories often provide metrics for a single software component.

By deriving from `CounterCategoryBase<TCategory>` your category can be acquired from a cache via `CounterCategoryBase<TCategory>.Factory.GetOrCreate(...)`.

1. Define your counter fields as `public readonly` fields on your category object.
1. Define your counter fields as concrete types, ie. use `MeanAverage` not `ICounter<long>`.
1. Define a protected constructor which calls the base constructor of the same signature (see example.)

### Example: Queue Statistics Category

```csharp
public sealed class QueueStatistics :
    CounterCategoryBase<QueueStatistics>
{    
    public readonly MeanAverage AverageTimeInQueue;
    
    public readonly SumTotal InTotal;
    
    public readonly RatePerSecond InPerSec;

    public readonly SumTotal OutTotal;

    public readonly RatePerSecond OutPerSec;
    
    protected QueueStatistics(string instanceName)
        : base(instanceName)
    { }
}
```

### Why does `CounterCategoryBase` have a type parameter?

This allows us to define static members which are unique between subclasses but where the behavior is the same, such as 'named instance lists.'


## Base Counters

A **Base Counter** is typically used as a secondary component in counter value calculations, such as a denominator or multiplier. For example, `ItemsPerSecond64` is a subclass of `MeanAverage64` which uses an `ElapsedTime64` counter as a **BaseCounter** during construction.

