using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using FileStorageApi.Infrastructure;
using Serilog;

namespace FileStorageApi;

class Program
{
	private static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Services.AddFileStorageApiServices(builder.Configuration);
		
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
