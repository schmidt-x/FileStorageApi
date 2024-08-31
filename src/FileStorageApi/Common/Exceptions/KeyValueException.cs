using System;

namespace FileStorageApi.Common.Exceptions;

public class KeyValueException : Exception
{
	public string Key { get; }
	public string Value { get; }
	
	public KeyValueException(string key, string value) : base($"{key}: {value}")
	{
		Key = key;
		Value = value;
	}
}