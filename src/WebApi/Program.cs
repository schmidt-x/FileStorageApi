using FileStorageApi.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using WebApi.Infrastructure;
using Serilog;

namespace WebApi;

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
			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStorageApi v1");
			});
		}
		
		app.MapEndpoints();
		
		app.Run();
	}
}