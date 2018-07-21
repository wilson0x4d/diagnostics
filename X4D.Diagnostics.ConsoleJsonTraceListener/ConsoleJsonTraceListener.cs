using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace X4D.Diagnostics
{
    public sealed class ConsoleJsonTraceListener :
        JsonTextWriterTraceListener
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

        public static void SetConsoleColor(TraceEventType traceEventType)
        {
            if (!ConsoleJsonTraceListener.Settings.ShouldColorizeOutput)
            {
                return;
            }
            switch (traceEventType)
            {
                case TraceEventType.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case TraceEventType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;

                case TraceEventType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case TraceEventType.Resume:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case TraceEventType.Start:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case TraceEventType.Stop:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case TraceEventType.Suspend:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case TraceEventType.Transfer:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case TraceEventType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case TraceEventType.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                default:
                    Console.ResetColor();
                    break;
            }
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
                // before being sent to the console. This can happen if
                // you've added this listener to the `<trace><listeners/>` section.
                message = BuildJsonEventData(
                    DateTime.UtcNow,
                    default(string),
                    TraceEventType.Verbose,
                    0,
                    message);
            }
            Console.WriteLine(message);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            SetConsoleColor(eventType);
            base.TraceData(eventCache, source, eventType, id, data);
            Console.ResetColor();
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            SetConsoleColor(eventType);
            base.TraceData(eventCache, source, eventType, id, data);
            Console.ResetColor();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            SetConsoleColor(eventType);
            base.TraceEvent(eventCache, source, eventType, id);
            Console.ResetColor();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            SetConsoleColor(eventType);
            base.TraceEvent(eventCache, source, eventType, id, format, args);
            Console.ResetColor();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            SetConsoleColor(eventType);
            base.TraceEvent(eventCache, source, eventType, id, message);
            Console.ResetColor();
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            SetConsoleColor(TraceEventType.Transfer);
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
            Console.ResetColor();
        }

        public static class Settings
        {
            public static bool ShouldColorizeOutput { get; set; } = true;
        }
    }
}
