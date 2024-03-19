using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileStorageApi.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
	{
		// Adding Serilog
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(config)
			.CreateLogger();
		
		services.AddSerilog(Log.Logger, true);
		
		
		return services;
	}
}