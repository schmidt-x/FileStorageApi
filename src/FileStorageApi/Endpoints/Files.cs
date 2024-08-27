using FileStorageApi.Features.Files.Commands.CreateFile;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Exceptions.FileExceptions;
using FileStorageApi.Common.Exceptions;
using FileStorageApi.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileStorageApi.Responses;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace FileStorageApi.Endpoints;

public class Files : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		var files = app
			.MapGroup("/api/files")
			.WithTags("Files")
			.DisableAntiforgery()    // TODO: remove
			.RequireAuthorization();
		
		files
			.MapPost("/", CreateFile)
			.Produces(StatusCodes.Status201Created)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status413PayloadTooLarge)
			.Produces(StatusCodes.Status401Unauthorized);
	}
	
	public async Task<IResult> CreateFile(
		[FromForm] IFormFile file,
		[FromForm] string? folder,
		CreateFileCommandHandler handler,
		CancellationToken ct)
	{
		await using var fileStream = file.OpenReadStream();
		
		var command = new CreateFileCommand(fileStream, file.FileName, file.ContentType, folder);
		
		// TODO: use Option<T>?
		Exception? ex = await handler.Handle(command, ct);
		
		if (ex is null) return Results.Created();
		
		return Results.BadRequest(ex switch
		{
			ValidationException vEx => new FailResponse(vEx.Errors),
			FileException           => new FailResponse("File", ex.Message),
			FolderException         => new FailResponse("Folder", ex.Message),
			_                       => new FailResponse("Error", ex.Message)
		});
	}
	
}
