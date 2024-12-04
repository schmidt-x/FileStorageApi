using FileStorageApi.Common.Models;
using System.Collections.Generic;

namespace FileStorageApi.Responses;

public class FailResponse
{
	public IList<ErrorDetail> Errors { get; }
	
	public FailResponse(IList<ErrorDetail> errors) => Errors = errors;

	public FailResponse(ErrorDetail error) : this([ error ])
	{ }
}