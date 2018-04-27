using System.Diagnostics;

namespace Saber.Common.ProcessInfo
{
    public class Gulp
    {
        public static void OutputReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            //Console.WriteLine(e.Data);
        }

        public static void ErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            //Console.WriteLine(e.Data);
        }
    }
}
