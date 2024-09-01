using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FileStorageApi.Common.Exceptions;
using FileStorageApi.Features.Folders.Commands.CreateFolder;
using FileStorageApi.Infrastructure;
using FileStorageApi.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApi.Endpoints;

public class Folders : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		var folders = app
			.MapGroup("/api/folders")
			.WithTags("Folders")
			.DisableAntiforgery()    // TODO: remove
			.RequireAuthorization();
		
		folders
			.MapPost("/", CreateFolder)
			.Produces<CreatedFolder>(StatusCodes.Status201Created)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized);
		
		folders
			.MapGet("/{*folderName}", GetFolder)
			.WithName("GetFolder");
	}
	
	public async Task<IResult> CreateFolder(
		[FromForm] string folderName,
		CreateFolderCommandHandler handler,
		CancellationToken ct)
	{
		var createFolderResult = await handler.Handle(new CreateFolderCommand(folderName), ct);
		
		return createFolderResult.Match<IResult>(
			
			createdFolder => Results.CreatedAtRoute(
				nameof(GetFolder),
				new { folderName = createdFolder.FullName[1..] },
				createdFolder),
			
			ex => Results.BadRequest(ex switch
			{
				ValidationException vEx => new FailResponse(vEx.Errors),
				KeyValueException kvEx  => new FailResponse(kvEx.Key, kvEx.Value),
				_                       => new FailResponse("Error", ex.Message)
			}));
	}
	
	public Task<IResult> GetFolder(string folderName)
	{
		folderName = HttpUtility.UrlDecode(folderName);
		
		throw new NotImplementedException();
	}
}