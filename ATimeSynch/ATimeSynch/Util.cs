using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ATimeSynch
{
    internal class Util
    {
        public static DateTime GetNetworkTime()
        {
            var sw = new Stopwatch();
            sw.Start();
            const string ntpServer = "3.asia.pool.ntp.org";
            var ntpData = new byte[48];
            ntpData[0] = 0x1B;
            IPAddress[] addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            socket.ReceiveTimeout = 3000;
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();
            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);
            ulong milliseconds = (intPart*1000) + ((fractPart*1000)/0x100000000L);
            sw.Stop();
            milliseconds += Convert.ToUInt64(sw.ElapsedMilliseconds);
            DateTime networkDateTime =
                (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long) milliseconds);
            return networkDateTime;
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint) (((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

        public struct SYSTEMTIME
        {
            public ushort Day;
            public ushort DayOfWeek;
            public ushort Hour;
            public ushort Milliseconds;
            public ushort Minute;
            public ushort Month;
            public ushort Second;
            public ushort Year;
        }
    }
}