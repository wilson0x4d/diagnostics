namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// An <see cref="ICounter{T}"/>, you can read a value of type <typeparamref name="T"/> from it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICounter<T>
    {
        /// <summary>
        /// Gets the Counter Value.
        /// </summary>
        T Value { get; }
    }
}
