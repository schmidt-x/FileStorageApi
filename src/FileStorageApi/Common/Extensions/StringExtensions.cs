using System;

namespace FileStorageApi.Common.Extensions;

public static class StringExtensions
{
	public static bool ContainsControlChars(this string str)
	{
		return str is not null
			? str.AsSpan().ContainsAnyInRange('\x00', '\x1F')
			: throw new ArgumentNullException(nameof(str));
	}
}