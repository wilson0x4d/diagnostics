using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace X4D.Diagnostics
{
    /// <summary>
    /// <para>adapted from https://gist.github.com/wilson0x4d/8704422</para>
    /// </summary>
    public class UdpJsonTraceListener
        : JsonTextWriterTraceListener
    {
        private readonly UdpClient _udpClient = new UdpClient();

        private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

        private StringBuilder _messageBuilder = new StringBuilder();
         
        public UdpJsonTraceListener()
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

        public UdpJsonTraceListener(string hostName, int portNumber)
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
                // before being sent to the server. This can happen if you've
                // added this listener to the `<trace><listeners/>` section.
                message = base.BuildJsonEventData(
                    DateTime.UtcNow,
                    "TRACE",
                    TraceEventType.Verbose,
                    0,
                    message);
            }
            byte[] payload = Encoding.UTF8.GetBytes(message);
            _udpClient.Send(payload, payload.Length);
        }

        public override void Flush()
        {
            base.Flush();
        }

        protected override string[] GetSupportedAttributes()
        {
            return base.GetSupportedAttributes()
                .Concat(new[]
                {
                    "initializeData"
                })
                .Distinct()
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
    }
}
