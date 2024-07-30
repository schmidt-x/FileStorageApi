using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using FluentMigrator.Runner;
using System.Reflection;
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
		
		// Adding FluentMigrator.
		services
			.AddFluentMigratorCore()
			.ConfigureRunner(rb => rb
				.AddPostgres()
				.WithGlobalConnectionString(sp => sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Postgres)
				.ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
			.AddLogging(lb => lb.AddFluentMigratorConsole());
		
		
		return services;
	}
}
