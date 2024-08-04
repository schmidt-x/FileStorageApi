using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;
using System;

namespace App.Common.Exceptions;

public class ValidationException : Exception
{
	public ValidationException() : base("One or more validation failures have occurred.")
	{
		Errors = new Dictionary<string, string[]>();
	}
	
	public ValidationException(IEnumerable<ValidationFailure> failures) : this()
	{
		Errors = failures
			.GroupBy(f => f.PropertyName, f => f.ErrorMessage)
			.ToDictionary(fGroup => fGroup.Key, fGroup => fGroup.ToArray());
	}
	
	public ValidationException(string propertyName, string[] errorMessages) : this()
	{
		Errors = new Dictionary<string, string[]> { { propertyName, errorMessages } };
	}
	
	public IDictionary<string, string[]> Errors { get; }
}