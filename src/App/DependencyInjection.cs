using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using App.Infrastructure;
using System.Reflection;
using FluentValidation;
using App.Services;
using App.Data;
using System;
using Dapper;

namespace App;

public static class DependencyInjection
{
	public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
	{
		services
			.AddSerilog(config)
			.AddConnectionStringsOptions()
			.AddStorageOptions()
			.AddFluentMigrator()
			.AddRequestHandlersFromExecutingAssembly()
			.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
			.AddSingleton(TimeProvider.System)
			.AddNpgsql()
			.AddSingleton<IPasswordHasher, PasswordHasher>()
			.AddScoped<IRepositoryService, RepositoryService>();
		
		DefaultTypeMap.MatchNamesWithUnderscores = true;
		
		return services;
	}
}
