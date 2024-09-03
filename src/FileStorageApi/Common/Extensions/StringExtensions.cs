using System;

namespace FileStorageApi.Common.Extensions;

public static class StringExtensions
{
	public static bool ContainsControlChars(this string str)
		=> str is null
			? throw new ArgumentNullException(nameof(str))
			: str.Length != 0 && str.AsSpan().ContainsAnyInRange('\x00', '\x1F');
}