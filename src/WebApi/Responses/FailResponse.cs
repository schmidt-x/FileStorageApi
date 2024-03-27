using System.Collections.Generic;

namespace WebApi.Responses;

public class FailResponse
{
	public FailResponse(string message) 
		=> Message = message;

	public FailResponse(string message, IDictionary<string, string[]> errors)
	 => (Message, Errors) = (message, errors); 
	
	public string Message { get; set; }
	public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}