using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.TraceListeners
{
    [TestClass]
    public sealed class UdpTraceListenerTests
    {
        [TestMethod]
        public void UdpTraceListener_BasicVerification()
        {
            var expectedText = Guid.NewGuid().ToString();
            using (var udpServer = new UdpClient(514))
            {
                udpServer.Client.ReceiveTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                expectedText.Log();
                var remoteHostEndPoint = new IPEndPoint(IPAddress.Any, 12345);
                var buf = udpServer.Receive(ref remoteHostEndPoint);
                var eventData = Encoding.UTF8.GetString(buf, 0, buf.Length);
                Assert.IsTrue(eventData.Contains(expectedText));
            }
        }
    }
}
