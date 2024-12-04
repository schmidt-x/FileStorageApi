using FileStorageApi.Common.Models;
using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;
using System;

namespace FileStorageApi.Common.Exceptions;

public class ValidationException : Exception
{
	public List<ErrorDetail> Errors { get; } = null!;
	
	private ValidationException() : base("One or more validation failures have occurred.")
	{ }
	
	public ValidationException(List<ValidationFailure> failures) : this()
	{
		Errors = new List<ErrorDetail>(failures.Count);
		
		Errors.AddRange(failures.Select(f => new ErrorDetail(
			(string)f.FormattedMessagePlaceholderValues["PropertyName"], f.PropertyName, f.ErrorMessage)));
	}
	
	public ValidationException(string code, string property, string description) : this()
	{
		Errors = [ new ErrorDetail(code, property, description) ];
	}
}