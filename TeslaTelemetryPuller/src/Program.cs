namespace TeslaTelemetryPuller
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private static bool shouldBreak = false;

        private static TeslaTelemetryPullerServiceConfig ParseArgs(string[] args)
        {
            var config = new TeslaTelemetryPullerServiceConfig();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
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
            Console.CancelKeyPress += new ConsoleCancelEventHandler(BreakHandler);

            var config = ParseArgs(args);
            if (config == null)
            {
                ShowUsage();
                return -1;
            }

            var service = new TeslaTelemetryPullerService(config);

            service.Initialize();

            var serivceTask = Task.Run(() => service.Run());

            if (!bool.Parse(config["runonce"]))
            {
                while (!shouldBreak)
                {
                    // Keep running.
                    Thread.Sleep(1000);
                }

                service.Stop();
            }

            serivceTask.Wait();

            return 0;
        }

        private static void BreakHandler(object sender, ConsoleCancelEventArgs args)
        {
            shouldBreak = true;
        }
    }
}
