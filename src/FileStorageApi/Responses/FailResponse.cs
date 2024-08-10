using System.Collections.Generic;

namespace FileStorageApi.Responses;

public class FailResponse
{
	public FailResponse(IDictionary<string, string[]> errors)
	{
		Errors = errors;
	}
	
	public FailResponse(string key, string[] errors)
		: this(new Dictionary<string, string[]> { { key, errors } })
	{	}
	
	public FailResponse(string key, string error)
		: this(new Dictionary<string, string[]> { { key, [error] } })
	{	}
	
	public IDictionary<string, string[]> Errors { get; }
}