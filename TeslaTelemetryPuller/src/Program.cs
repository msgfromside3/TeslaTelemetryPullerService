using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaTelemetryPuller
{
    class Program
    {

        private static TeslaTelemetryPullerServiceConfig ParseArgs(string[] args)
        {
            var config = new TeslaTelemetryPullerServiceConfig();

            for(int i = 0; i < args.Length; i++)
            {
                switch(args[i].ToLowerInvariant())
                {
                    case "-runonce":
                        config.AddConfig("runonce", "true");
                        break;
                    default:
                        return null;
                }
            }

            return config;
        }

        private static void ShowUsage()
        {
            Console.Out.WriteLine("Usage:\tTeslaTelemetryPuller.exe [-runonce]");
        }

        static int Main(string[] args)
        {
            var config = ParseArgs(args);
            if (config == null)
            {
                ShowUsage();
                return -1;
            }

            var service = new TeslaTelemetryPullerService(config);

            return 0;
        }
    }
}
