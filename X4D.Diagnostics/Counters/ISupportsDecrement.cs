namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// An <see cref="ICounter{T}"/> that supports Decrementing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISupportsDecrement<T> :
        ICounter<T>
    {
        /// <summary>
        /// Decrement the Counter by the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to Decrement the Counter by.</param>
        /// <returns>The resulting Counter Value after Decrementing.</returns>
        T Decrement(T value);
    }
}
