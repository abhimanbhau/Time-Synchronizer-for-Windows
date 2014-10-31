using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace ATimeSynch
{
    class Program
    {
        public struct SYSTEMTIME
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;
        }

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public extern static bool Win32SetSystemTime(ref SYSTEMTIME sysTime);
        static void Main(string[] args)
        {
            if (!UacHelper.IsProcessElevated)
            {
                Console.WriteLine("Restart with admin privilages");
                return;
            }
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                DateTime ntpTime = Util.GetNetworkTime();
                var diff = DateTime.Now.Subtract(ntpTime.ToLocalTime()).TotalMilliseconds;
                Console.WriteLine("Time Drift(ms) -> " + diff);
                Console.WriteLine("System Time -> " + DateTime.Now + DateTime.Now.Millisecond);
                Console.WriteLine("Atomic Time -> " + ntpTime.ToLocalTime() + ntpTime.ToLocalTime().Millisecond);
                sw.Stop();
                var timeToSet = new SYSTEMTIME
                {
                    Year = (ushort)ntpTime.Year,
                    Month = (ushort)ntpTime.Month,
                    DayOfWeek = (ushort)ntpTime.DayOfWeek,
                    Day = (ushort)ntpTime.Day,
                    Hour = (ushort)ntpTime.Hour,
                    Minute = (ushort)ntpTime.Minute,
                    Second = (ushort)ntpTime.Second,
                    Milliseconds = (ushort)(ntpTime.Millisecond+sw.ElapsedMilliseconds)
                };
                Win32SetSystemTime(ref timeToSet);
                Console.WriteLine("Time sync with NTP Server successfull");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}

