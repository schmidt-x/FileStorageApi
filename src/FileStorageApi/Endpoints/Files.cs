using FileStorageApi.Features.Files.Queries.DownloadFile;
using FileStorageApi.Features.Files.Commands.CreateFile;
using FileStorageApi.Features.Files.Queries.GetFile;
using FileStorageApi.Common.Exceptions;
using FileStorageApi.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileStorageApi.Responses;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace FileStorageApi.Endpoints;

public class Files : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		var files = app.MapGroup("/api/files")
			.WithTags("Files")
			.DisableAntiforgery()    // TODO: remove
			.RequireAuthorization();
		
		files.MapPost("/", CreateFile)
			.Produces<CreatedFile>(StatusCodes.Status201Created)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status413PayloadTooLarge);
		
		files.MapGet("/info/{*fileName}", GetFile)
			.WithName(nameof(GetFile))
			.Produces<FileDto>()
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized);
		
		files.MapGet("/download/{*fileName}", DownloadFile)
			.Produces(StatusCodes.Status200OK)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
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
		
		var createFileResult = await handler.Handle(command, ct);
		
		return createFileResult.Match<IResult>(
			
			createdFile => Results.CreatedAtRoute(
				nameof(GetFile),
				new { fileName = createdFile.FullName[1..]},
				createdFile),
			
			ex => Results.BadRequest(ex switch
			{
				ValidationException vEx => new FailResponse(vEx.Errors),
				KeyValueException kvEx  => new FailResponse(kvEx.Key, kvEx.Value),
				_                       => new FailResponse("Error", ex.Message)
			}));
	}
	
	public async Task<IResult> GetFile(
		string fileName,
		GetFileQueryHandler handler,
		CancellationToken ct)
	{
		fileName = HttpUtility.UrlDecode(fileName);
		var getFileResult = await handler.Handle(new GetFileQuery(fileName), ct);
		
		return getFileResult.Match(
			Results.Ok,
			ex => Results.BadRequest(ex switch
			{
				ValidationException vEx => new FailResponse(vEx.Errors),
				KeyValueException kvEx  => new FailResponse(kvEx.Key, kvEx.Value),
				_                       => new FailResponse("Error", ex.Message)
			}));
	}
	
	public async Task<IResult> DownloadFile(
		string fileName,
		DownloadFileQueryHandler handler,
		CancellationToken ct)
	{
		fileName = HttpUtility.UrlDecode(fileName);
		
		var downloadFileResult = await handler.Handle(new DownloadFileQuery(fileName), ct);
		
		return downloadFileResult.Match<IResult>(
			file => Results.File(file.Stream, file.MimeType, file.Name),
			ex => Results.BadRequest(ex switch
			{
				ValidationException vEx => new FailResponse(vEx.Errors),
				KeyValueException kvEx  => new FailResponse(kvEx.Key, kvEx.Value),
				_                       => new FailResponse("Error", ex.Message)
			}));
	}
}
