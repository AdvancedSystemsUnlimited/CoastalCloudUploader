using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace AttachmentUpload
{
	class Program
	{
		private readonly IServiceProvider serviceProvider;
		private static ArgService argService;
		private static ServiceCollection serviceCollection = new ServiceCollection();
		private static string baseDirectory = AppContext.BaseDirectory;
		private static string[] args;
		public static void Main(string[] _args)
		{
			args = _args ?? new string[] { };
			bool bPrepareFile = false;
			// Create service collection
			ConfigureServices(serviceCollection);

			// Create service provider
			var serviceProvider = serviceCollection.BuildServiceProvider();
			string[] vbsArgs = new string[] { };
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "-f")
				{
					bPrepareFile = true;
					// call http client args[i+1] for URL
					vbsArgs[0] = args[i + 1];
					serviceProvider.GetService<App>().Run(vbsArgs);
				}
			}
			if(!bPrepareFile){
				serviceProvider.GetService<App>().Run(args);
			}
		}
		static bool PrepareFile()
		{
			bool filePrepared ;
			try
			{
				//Process.Start(@"cscript //B //Nologo prepareFile.vbs");
				//	For More Specific Process Control
				Process scriptProc = new Process();
				scriptProc.StartInfo.FileName = @"cscript";
				scriptProc.StartInfo.WorkingDirectory = @"."; //<---very important 
				scriptProc.StartInfo.Arguments = "//B //Nologo prepareFile.vbs";
				scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //prevent console window from popping up
				scriptProc.Start();
				scriptProc.WaitForExit(); // <-- Optional if you want program running until your script exit
				scriptProc.Close();
				filePrepared = true;
			}
			catch(Exception ex)
			{
				filePrepared = false;
			}
			return filePrepared;
		}
		private static void ConfigureLogger(IServiceCollection _serviceCollection)
		{
			_serviceCollection.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddSerilog();
				//loggingBuilder.AddDebug();
			});
			_serviceCollection.AddLogging();
			// Initialize serilog logger
			Log.Logger = new LoggerConfiguration()
				.WriteTo.RollingFile($@"{baseDirectory}\log\log.txt", retainedFileCountLimit: 10)
				.MinimumLevel.Debug()
				.Enrich.FromLogContext()
				.CreateLogger();
		}

		private static void ConfigureServices(IServiceCollection _serviceCollection)
		{
			// Add logging
			ConfigureLogger(_serviceCollection);

			_serviceCollection.AddTransient<IArgService, ArgService>();

			// Build configuration
			var configuration = new ConfigurationBuilder()
				.SetBasePath(baseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();
			// Add access to generic IConfigurationRoot
			_serviceCollection.AddSingleton(configuration);

			ConnectAPI configAPI = new ConnectAPI();
			configuration.GetSection("connectapi").Bind(configAPI);
			_serviceCollection.AddSingleton(configAPI);

			// Add app
			serviceCollection.AddTransient<App>();
		}
	}
}
