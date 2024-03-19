using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using FileStorageApi.Application;
using Serilog;

namespace FileStorageApi.WebApi;

public class Program
{
	public static void Main()
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		
		builder.Services
			.AddApplicationServices(builder.Configuration)
			.AddWebApiServices();
		
		WebApplication app = builder.Build();

		app.UseSerilogRequestLogging();
		
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		
		app.Run();
	}
}