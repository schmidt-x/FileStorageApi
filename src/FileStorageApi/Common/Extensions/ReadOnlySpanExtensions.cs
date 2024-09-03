using System;

namespace FileStorageApi.Common.Extensions;

public static class ReadOnlySpanExtensions
{
	public static bool ContainsControlChars(this ReadOnlySpan<char> chars)
		=> !chars.IsEmpty && chars.ContainsAnyInRange('\x00', '\x1F');
}