namespace X4D.Diagnostics.Counters
{
    /// <summary>
    /// An object that supports Reset of state.
    /// <para>What state is reset depends on each implementation.</para>
    /// <para>
    /// Typically a reset changes the Counter Value and also calls <see
    /// cref="Reset"/> on any Base Counters.
    /// </para>
    /// </summary>
    public interface ISupportsReset
    {
        /// <summary>
        /// Reset Counter State.
        /// </summary>
        void Reset();
    }
}
