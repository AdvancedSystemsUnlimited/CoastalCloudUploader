using System;
using System.Collections.Generic;
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


namespace ThreadUpload
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
			args = _args;
			// Create service collection
			ConfigureServices(serviceCollection);

			// Create service provider
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// Run app
			serviceProvider.GetService<App>().Run(args);
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
