using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using App.Infrastructure;
using App.Data;
using System;

namespace App;

public static class DependencyInjection
{
	public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
	{
		services
			.AddSerilog(config)
			.AddConnectionStringsOptions()
			.AddFluentMigrator()
			.AddRequestHandlersFromExecutingAssembly()
			.AddSingleton(TimeProvider.System)
			.AddNpgsql()
			.AddScoped<IRepositoryService, RepositoryService>();
		
		return services;
	}
}
