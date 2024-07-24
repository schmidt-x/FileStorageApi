using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using App.Options;
using Serilog;

namespace App;

public static class DependencyInjection
{
	public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
	{
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(config)
			.CreateLogger();
		
		// Adding Serilog logger and replacing the built-in one.
		services.AddSerilog(Log.Logger, true);
		
		// Adding database connection string.
		services
			.AddOptions<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConnectionStrings)
			.Validate(o => !string.IsNullOrWhiteSpace(o.Postgres), "Connection string is required.")
			.ValidateOnStart();
		
		
		return services;
	}
}