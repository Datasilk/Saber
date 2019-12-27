using System.Diagnostics;

namespace Saber.Common.ProcessInfo
{
    public static class Gulp
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

        public static void Task(string taskName)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c gulp " + taskName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Server.MapPath("/").Replace("App\\", ""),
                    Verb = "runas"
                }
            };
            p.OutputDataReceived += Common.ProcessInfo.Gulp.OutputReceived;
            p.ErrorDataReceived += Common.ProcessInfo.Gulp.ErrorReceived;
            p.Start();
            p.WaitForExit();
        }
    }
}
