using FileStorageApi.Data.TypeHandlers;
using System;
using System.Reflection;
using Dapper;
using FileStorageApi.Data;
using FileStorageApi.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using FileStorageApi.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileStorageApi;

class Program
{
	private static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Services.AddFeatures();
		builder.Services.AddCommonServices();
		builder.Services.AddSerilog(builder.Configuration);
		
		builder.Services
			.AddConnectionStringsOptions()
			.AddStorageOptions()
			.AddAuthOptions();
		
		builder.Services.AddFluentMigrator();
		builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			
		builder.Services.AddSingleton(TimeProvider.System);
		
		builder.Services.AddNpgsql();
		DefaultTypeMap.MatchNamesWithUnderscores = true;
		SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
		
		builder.Services.AddScoped<IRepositoryContext, RepositoryContext>();
		
		builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();
		
		builder.Services.AddCookieAuthentication();
		builder.Services.AddAuthorization();
		
		builder.Services.AddHttpContextAccessor();
		
		var app = builder.Build();
		
		app.RunMigrations();
		
		app.UseSerilogRequestLogging();
		
		app.UseHttpsRedirection();
		
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		
		app.UseAuthentication();
		app.UseAuthorization();
		
		app.MapEndpoints();
		
		app.Run();
	}
}
