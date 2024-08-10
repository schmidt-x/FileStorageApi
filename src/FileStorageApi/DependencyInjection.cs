using System;
using System.Reflection;
using Dapper;
using FileStorageApi.Services;
using Microsoft.Extensions.DependencyInjection;
using FileStorageApi.Infrastructure;
using FileStorageApi.Common.Interfaces;
using FileStorageApi.Data;
using FileStorageApi.Features;
using FileStorageApi.Features.Auth.Contracts;
using FluentValidation;
using Microsoft.Extensions.Configuration;


namespace FileStorageApi;

public static class DependencyInjection
{
	public static IServiceCollection AddFileStorageApiServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddFeatures();
		
		services.AddSerilog(config);
		
		services
			.AddConnectionStringsOptions()
			.AddStorageOptions();
		
		services.AddFluentMigrator();
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			
		services.AddSingleton(TimeProvider.System);
		
		services.AddNpgsql();
		DefaultTypeMap.MatchNamesWithUnderscores = true;
		
		services.AddScoped<IRepositoryContext, RepositoryContext>();
		
		services.AddEndpointsApiExplorer().AddSwaggerGen();
		
		services.AddCookieAuthentication();
		services.AddAuthorization();
		
		services.AddSingleton<IAuthSchemeProvider, CookieAuthSchemeProvider>();
		services.AddScoped<IUser, CurrentUser>();
		
		services.AddHttpContextAccessor();
		
		return services;
	}
}