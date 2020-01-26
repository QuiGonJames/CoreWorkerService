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

        public Worker(ILogger<Worker> logger,
                      IConfiguration configuration)
        {
            _logger = logger;
            _appSettings = configuration.GetSection("ServiceControlSettings");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}",
                                       DateTimeOffset.Now);
				LogSomething($"Logged message: Worker running at: {DateTimeOffset.Now}");
                await Task.Delay(_appSettings.GetValue<int>("IntervalSecs"), stoppingToken);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
		private void LogSomething(string logMessage)
		{
			try
			{
				var filePath = ".\\Workfile.txt";

				if (!Directory.Exists(filePath))
				{
					_logger.LogError("Couldn't find path to file");
					return;
				}

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
					var errMsg = string.Format("The following fatal {0} was caught in EmvTerminalSettingsService.LogError(){1}{2}{1}StackTrace: {3}{1}Inner Exception: {4}",
											   ex.GetType(),
											   Environment.NewLine,
											   ex.Message,
											   ex.StackTrace,
											   ex.InnerException == null
												   ? "N/A"
												   : ex.InnerException.Message);
					errMsg += Environment.NewLine + logMessage;
				}
				catch
				{
					// Eat errors here
				}
			}
		}
	}
}