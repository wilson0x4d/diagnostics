namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// An <see cref="ICounter{T}"/> that supports Incrementing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISupportsIncrement<T> :
        ICounter<T>
    {
        /// <summary>
        /// Increment the Counter by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to Increment the Counter by.</param>
        /// <returns>The resulting Counter Value after Incrementing.</returns>
        T Increment(T value);
    }
}
