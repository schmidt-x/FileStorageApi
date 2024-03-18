using FileStorageApi.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace FileStorageApi.WebApi;

public class Program
{
	public static void Main()
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		
		builder.Services
			.AddApplicationServices()
			.AddWebApiServices();
		
		WebApplication app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		
		app.Run();
	}
}