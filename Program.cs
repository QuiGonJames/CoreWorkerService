using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;

namespace James
{
    public static class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        // Determine linux systemd or windows service and apply:
        public static IHostBuilder CreateHostBuilder(string[] args) => GetHostOsInfo() == OSPlatform.Windows
            ? Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) => services.AddHostedService<Worker>()).UseWindowsService()
            : Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) => services.AddHostedService<Worker>()).UseConsoleLifetime();

        private static OSPlatform GetHostOsInfo()
        {
            var currentOs = Environment.OSVersion;

            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? OSPlatform.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                    ? OSPlatform.FreeBSD
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                                    ? OSPlatform.Linux
                                    : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                                                    ? OSPlatform.OSX
                                                    : OSPlatform.Create("Unknown");
        }
    }
}