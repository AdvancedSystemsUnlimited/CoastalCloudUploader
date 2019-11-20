using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace TestWebAPI
{
	public class Startup
	{
		private readonly ILogger<Startup> _logger;
		private static string baseDirectory = AppContext.BaseDirectory;
		private static string[] args;
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		private static void ConfigureLogger(IServiceCollection _serviceCollection)
		{
			_serviceCollection.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddConsole();
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
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
		}
	}
}
