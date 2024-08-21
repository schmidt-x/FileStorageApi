using FileStorageApi.Common.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security;
using System;

namespace FileStorageApi.Services;

public class CurrentUser : IUser
{
	private readonly HttpContext _context;
	
	public CurrentUser(IHttpContextAccessor contextAccessor)
	{
		_context = contextAccessor.HttpContext!;
	}
	
	public Guid Id()
	{
		if (_context.User.Identity is null || !_context.User.Identity.IsAuthenticated)
		{
			throw new InvalidOperationException("User is not authenticated.");
		}
		
		var rawId = _context.User.FindFirstValue(ClaimTypes.NameIdentifier) 
		            ?? throw new SecurityException("Claim 'NameIdentifier' is not present.");
		
		if (!Guid.TryParse(rawId, out var id))
		{
			throw new SecurityException("Claim 'NameIdentifier' is not valid Guid value.");
		}
		
		return id;
	}
}