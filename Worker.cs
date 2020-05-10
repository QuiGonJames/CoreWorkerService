using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace James
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfigurationSection _appSettings;
        private bool _serviceRunning;

        public Worker(ILogger<Worker> logger,
                      IConfiguration configuration)
        {
            _logger = logger;
            _appSettings = configuration?.GetSection("ServiceControlSettings");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceRunning = true;

            while (!stoppingToken.IsCancellationRequested && _serviceRunning)
            {
                var timeToWait = DateTime.Now.AddSeconds(_appSettings.GetValue<int>("IntervalSecs"));

                _logger.LogInformation("Worker running at: {time}",
                                       DateTimeOffset.Now);
                LogSomething($"Logged message: Worker running at: {DateTimeOffset.Now}");

                while (timeToWait > DateTime.Now && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(100,
                                     stoppingToken).ConfigureAwait(true);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    LogSomething($"Logged message: Worker process cancelled at : {DateTimeOffset.Now}");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var timeToWait = DateTime.Now.AddMilliseconds(300);

            _serviceRunning = false;

            while (timeToWait > DateTime.Now && cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100,
                                 cancellationToken).ConfigureAwait(true);
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
        private void LogSomething(string logMessage)
        {
            try
            {
                var filePath = $"{AppDomain.CurrentDomain.BaseDirectory}";

                if (!Directory.Exists(filePath))
                {
                    try
                    {
                        var result = Directory.CreateDirectory(filePath);

                        if (!result.Exists)
                        {
                            _logger.LogError($"Couldn't find path to log file: {filePath}");
                            return;
                        }
                    }
                    catch (IOException)
                    {
                        // This will probably fail b/c the error is in created the path to this, but just playing right now
                        _logger.LogError($"Error creating directory: {filePath}");
                        return;
                    }
                    catch (Exception)
                    {
                        // TODO: Add general exception handling
                        throw;
                    }
                }

                filePath += "\\Workfile.txt";

                if (File.Exists(filePath))
                {
                    var fileData = new FileInfo(filePath);

                    if (fileData.Length > 1048576)
                    {
                        var startingIndex = fileData.Length / 2;
                        var newLineList = new List<string>();

                        if (startingIndex > int.MaxValue)
                        {
                            startingIndex = int.MaxValue;
                        }

                        using (var sw = File.OpenText(filePath))
                        {
                            sw.BaseStream.Position = startingIndex;

                            while (!sw.EndOfStream)
                            {
                                newLineList.Add(sw.ReadLine());
                            }
                        }

                        using (var sw = File.CreateText(filePath))
                        {
                            foreach (var fileLine in newLineList)
                            {
                                sw.WriteLine(fileLine);
                            }

                            sw.WriteLine($"[{DateTime.Now:MM/dd/yyyy HH:mm:ss.ff}]:");
                            sw.WriteLine(logMessage);
                        }
                    }
                    else
                    {
                        using (var sw = File.AppendText(filePath))
                        {
                            sw.WriteLine($"[{DateTime.Now:MM/dd/yyyy HH:mm:ss.ff}]:");
                            sw.WriteLine(logMessage);
                            sw.Close();
                        }
                    }
                }
                else
                {
                    using (var sw = File.CreateText(filePath))
                    {
                        sw.WriteLine($"[{DateTime.Now:MM/dd/yyyy HH:mm:ss.ff}]:");
                        sw.WriteLine(logMessage);
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var errMsg = $"The following fatal {ex.GetType()} was caught in WorkerService.LogError(){Environment.NewLine}{ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}{Environment.NewLine}Inner Exception: {(ex.InnerException == null ? "N/A" : ex.InnerException.Message)}";
                    errMsg += Environment.NewLine + logMessage;
                    _logger.LogError(errMsg);
                }
                catch
                {
                    // Eat errors here
                }
            }
        }
    }
}