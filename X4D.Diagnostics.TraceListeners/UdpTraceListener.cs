using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace X4D.Diagnostics.TraceListeners
{
    /// <summary>
    /// <para>adapted from https://gist.github.com/wilson0x4d/8704422</para>
    /// </summary>
    public class UdpTraceListener
        : TraceListener
    {
        private readonly int _processId = Process.GetCurrentProcess().Id;

        private readonly UdpClient _udpClient = new UdpClient();

        private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Used to collect "event data" (header, content, footer) and
        /// produce a single "event message".
        /// </summary>
        private StringBuilder _messageBuilder = new StringBuilder();

        private char[] _crlfChars = Environment.NewLine.ToCharArray();

        public UdpTraceListener()
        {
            var initializeData = base.Attributes["initializeData"] as string;
            var parts = initializeData?.Split(':');
            var udpHostName = "localhost";
            if (parts?.Length > 0)
            {
                udpHostName = parts[0];
            }
            var udpPortNumber = 514;
            if (parts?.Length > 1 && int.TryParse(parts[1], out int portNumber))
            {
                udpPortNumber = portNumber;
            }
            Connect(udpHostName, udpPortNumber);
        }

        public UdpTraceListener(string hostName, int portNumber)
        {
            Connect(hostName, portNumber);
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public override void Write(string message)
        {
            Write(message, "");
        }

        public override void WriteLine(string message)
        {
            WriteLine(message, "");
        }

        public override void WriteLine(string message, string category)
        {
            var builder = _messageBuilder;
            _messageBuilder = new StringBuilder();
            builder.AppendLine(message);
            var eventMessage = GetEventString(builder.ToString(), category);
            _messageQueue.Enqueue(eventMessage);
            Flush();
        }

        public override void Write(string message, string category)
        {
            _messageBuilder.Append(message);
        }

        public override void Flush()
        {
            while (_messageQueue.Count > 0)
            {
                if (_messageQueue.TryDequeue(out string message))
                {
                    byte[] payload = Encoding.UTF8.GetBytes(message);
                    _udpClient.Send(payload, payload.Length);
                }
            }
            base.Flush();
        }

        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    WriteLine(string.Format(format, args), "error");
                    break;

                case TraceEventType.Verbose:
                    WriteLine(string.Format(format, args), "debug");
                    break;

                case TraceEventType.Warning:
                    WriteLine(string.Format(format, args), "warn");
                    break;

                case TraceEventType.Information:
                case TraceEventType.Resume:
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Transfer:
                default:
                    WriteLine(string.Format(format, args), "debug");
                    break;
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return base.GetSupportedAttributes()
                .Concat(new[]
                {
                    "initializeData"
                })
                .ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            _udpClient.Close();
            base.Dispose(disposing);
        }

        private void Connect(string hostName, int portNumber)
        {
            _udpClient.Connect(hostName, portNumber);
        }

        private string GetEventString(string message, string category)
        {
            message = ParseHeaderFromMessage(
                message,
                out string source,
                out string level,
                out string id);
            message = JsonEncodeString(message);
            return "{" +
                $@" ""ts"":""{DateTime.UtcNow:o}""," +
                $@" ""level"":""{level}""," +
                $@" ""message"":""{message}""," +
                $@" ""source"":""{source}""," +
                $@" ""id"":""{source}""," +
                $@" ""host"":""{Environment.MachineName}""," +
                $@" ""pid"":{_processId}," +
                $@" ""tid"":{System.Threading.Thread.CurrentThread.ManagedThreadId}" +
                " }";
        }

        private string ParseHeaderFromMessage(
            string message,
            out string source,
            out string level,
            out string id)
        {
            var headerParts = message.Split(new char[] { ' ' }, 4);
            source = headerParts.Length > 0
                ? headerParts[0]
                : "";
            level = headerParts.Length > 1
                ? headerParts[1].Remove(headerParts[1].Length - 1)
                : "";
            id = headerParts.Length > 2
                ? headerParts[2]
                : "";
            var messageIndex = message.IndexOf(" : ");
            return
                (messageIndex > 0
                    ? message.Substring(messageIndex + 3)
                    : message)
                .TrimEnd(_crlfChars);
        }

        private string JsonEncodeString(string message)
        {
            return message
                .Replace("\\", "\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
