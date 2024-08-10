using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FileStorageApi.Options;
using FileStorageApi.Options.Validation;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace FileStorageApi.Infrastructure;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddCookieAuthentication(this IServiceCollection services)
	{
		services
			.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				
				options.Events.OnRedirectToLogin = rc =>
				{
					rc.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return Task.CompletedTask;
				};
				
				options.Events.OnRedirectToAccessDenied = rc =>
				{
					rc.Response.StatusCode = StatusCodes.Status403Forbidden;
					return Task.CompletedTask;
				};
			});
		
		return services;
	}
	
	public static IServiceCollection AddConnectionStringsOptions(this IServiceCollection services)
	{
		services
			.AddOptions<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConnectionStrings)
			.Validate(o => !string.IsNullOrWhiteSpace(o.Postgres), "Connection string is required.")
			.ValidateOnStart();
		
		return services;
	}

	public static IServiceCollection AddStorageOptions(this IServiceCollection services)
	{
		services
			.AddOptions<StorageOptions>()
			.BindConfiguration(StorageOptions.Storage);
		
		services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();
		
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
	
	public static IServiceCollection AddNpgsql(this IServiceCollection services)
	{
		return services.AddSingleton<NpgsqlDataSource>(sp =>
		{
			var cnnString = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Postgres;
			return NpgsqlDataSource.Create(cnnString);
		});
	}
	
}