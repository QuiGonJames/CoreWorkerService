using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace James
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // TODO: Potential logic for determining linux systemd or windows service and apply:
        //public static IHostBuilder CreateHostBuilder(string[] args) => args[0].Equals("w")
        //    ? Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) => services.AddHostedService<Worker>()).UseWindowsService()
        //    : Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) => services.AddHostedService<Worker>()).UseConsoleLifetime();
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices((hostContext,
                                                               services) => services.AddHostedService<Worker>()).UseWindowsService();
    }
}