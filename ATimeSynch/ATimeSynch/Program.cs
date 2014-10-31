using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ATimeSynch
{
    internal class Program
    {
        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public static extern bool Win32SetSystemTime(ref Util.SYSTEMTIME sysTime);

        private static void Main(string[] args)
        {
            if (!UacHelper.IsProcessElevated)
            {
                Console.WriteLine("Restart with admin privilages");
                return;
            }
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                DateTime ntpTime = Util.GetNetworkTime();
                double diff = DateTime.Now.Subtract(ntpTime.ToLocalTime()).TotalMilliseconds;
                Console.WriteLine("Time Drift(ms) -> " + diff);
                Console.WriteLine("System Time -> " + DateTime.Now + DateTime.Now.Millisecond);
                Console.WriteLine("Atomic Time -> " + ntpTime.ToLocalTime() + ntpTime.ToLocalTime().Millisecond);
                sw.Stop();
                var timeToSet = new Util.SYSTEMTIME
                {
                    Year = (ushort) ntpTime.Year,
                    Month = (ushort) ntpTime.Month,
                    DayOfWeek = (ushort) ntpTime.DayOfWeek,
                    Day = (ushort) ntpTime.Day,
                    Hour = (ushort) ntpTime.Hour,
                    Minute = (ushort) ntpTime.Minute,
                    Second = (ushort) ntpTime.Second,
                    Milliseconds = (ushort) (ntpTime.Millisecond + sw.ElapsedMilliseconds)
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