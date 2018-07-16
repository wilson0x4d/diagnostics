using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace X4D.Diagnostics.TraceListeners
{
    public sealed class ConsoleJsonTraceListener :
        JsonWriterTraceListener
    {
        private StringBuilder _messageBuilder = new StringBuilder();

        public ConsoleJsonTraceListener()
        {
        }

        public ConsoleJsonTraceListener(string name) : base(name)
        {
        }

        public ConsoleJsonTraceListener(string name, JsonSerializerSettings serializerSettings) 
            : base(name, serializerSettings)
        {
        }

        public override void Write(string message)
        {
            _messageBuilder.Append(message);
        }

        public override void WriteLine(string message)
        {
            var messageBuilder = _messageBuilder;
            _messageBuilder = new StringBuilder();
            message = messageBuilder.Append(message).ToString();
            if (!message.StartsWith("{") || !message.EndsWith("}"))
            {
                // NOTE: this is performed to ensure non-JSON data is encoded
                // before being sent to the console. This can happen if you've
                // added this listener to the `<trace><listeners/>` section.
                message = BuildJsonEventData(
                    DateTime.UtcNow,
                    default(string),
                    TraceEventType.Verbose,
                    0,
                    message);
            }
            Console.WriteLine(message);
        }
    }
}
