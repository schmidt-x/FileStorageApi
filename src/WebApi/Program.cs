using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using WebApi.Infrastructure;
using Serilog;
using App;

namespace WebApi;

class Program
{
	private static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Services
			.AddAppServices(builder.Configuration)
			.AddWebApiServices();
		
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
