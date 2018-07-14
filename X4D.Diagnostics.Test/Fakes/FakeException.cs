using System;
using System.Runtime.Serialization;

namespace X4D.Diagnostics.Fakes
{
    [Serializable]
    internal class FakeException : Exception
    {
        public FakeException()
        {
        }

        public FakeException(string message) : base(message)
        {
        }

        public FakeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FakeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
