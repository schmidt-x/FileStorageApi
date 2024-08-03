using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using FluentMigrator.Runner;
using System.Reflection;
using System.Linq;
using App.Options;
using Serilog;
using Npgsql;

namespace App.Infrastructure;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddConnectionStringsOptions(this IServiceCollection services)
	{
		services
			.AddOptions<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConnectionStrings)
			.Validate(o => !string.IsNullOrWhiteSpace(o.Postgres), "Connection string is required.")
			.ValidateOnStart();
		
		return services;
	}
	
	public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration config)
	{
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(config)
			.CreateLogger();
		
		// Adding Serilog logger and replacing the built-in one.
		services.AddSerilog(Log.Logger, true);
		
		return services;
	}
	
	public static IServiceCollection AddFluentMigrator(this IServiceCollection services)
	{
		services
			.AddFluentMigratorCore()
			.ConfigureRunner(rb => rb
				.AddPostgres()
				.WithGlobalConnectionString(sp => sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Postgres)
				.ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
			.AddLogging(lb => lb.AddFluentMigratorConsole());
		
		return services;
	}
	
	public static IServiceCollection AddRequestHandlersFromExecutingAssembly(this IServiceCollection services)
	{
		var types = Assembly
			.GetExecutingAssembly()
			.GetExportedTypes()
			.Where(t => t is { IsClass: true, IsAbstract: false} && t.IsSubclassOf(typeof(RequestHandlerBase)));
		
		foreach (var type in types)
		{
			services.AddScoped(type);
		}
		
		return services;
	}
	
	public static IServiceCollection AddNpgsql(this IServiceCollection services)
	{
		return services.AddSingleton<NpgsqlDataSource>(sp =>
		{
			var cnnString = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Postgres;
			return NpgsqlDataSource.Create(cnnString);
		});
	}
	
	
}