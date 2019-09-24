using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor.Sample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var service = new Service1();

            if (Debugger.IsAttached)
            {
                service.Logger.LogLevel = AwsLogLevel.Verbose;
                service.DoStart();
                Console.WriteLine("Service started! Press ENTER to stop service...");
                Console.ReadLine();
                service.Stop();
            }
            else
            {
                ServiceBase.Run(service);
            }


        }
    }
}
