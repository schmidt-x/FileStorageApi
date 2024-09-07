using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FileStorageApi.Common.Exceptions;
using FileStorageApi.Domain.Enums;
using FileStorageApi.Features.Folders.Commands.CreateFolder;
using FileStorageApi.Features.Folders.Queries.GetFolder;
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
			.WithName("GetFolder")
			.Produces<FolderDto>()
			.Produces<FailResponse>(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized);
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
	
	public async Task<IResult> GetFolder(
		[FromRoute] string? folderName,
		[FromQuery] int? pageNumber,
		[FromQuery] int? pageSize,
		[FromQuery] ItemOrder? orderBy,
		[FromQuery] bool? desc,
		GetFolderQueryHandler handler,
		CancellationToken ct)
	{
		folderName = HttpUtility.UrlDecode(folderName);
		
		var query = new GetFolderQuery(
			folderName,
			pageNumber ?? 1,
			pageSize ?? 100,
			orderBy ?? ItemOrder.Type,
			desc ?? false);
			
		var getFolderResult = await handler.Handle(query, ct);
		
		return getFolderResult.Match<IResult>(
			Results.Ok,
			ex => Results.BadRequest(ex is KeyValueException kvEx
				? new FailResponse(kvEx.Key, kvEx.Value)
				: new FailResponse("Error", ex.Message))
		);
	}
}