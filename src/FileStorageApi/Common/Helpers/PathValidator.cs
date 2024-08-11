using FileStorageApi.Common.Exceptions;
using System.Linq;
using System.IO;

namespace FileStorageApi.Common.Helpers;


public class PathValidator
{
	
#if _WINDOWS
	private const string InvalidPrintableChars = "<>:\"|?*";
	
	// The only restricted symbol in Linux based systems is '/', but since it's used to separate path segments,
	// it doesn't make sense to consider it as such.
#endif  
	
	private static readonly string SeparatorStr = Path.DirectorySeparatorChar.ToString();
	private static readonly string DoubleSeparator = SeparatorStr + SeparatorStr;
	
	public static Result<string> Validate(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return new PathValidationException("Path is empty.");
		}
		
		// If we don't append separator at the end, Path.GetDirectoryName() will
		// concatenate the last part assuming it's a file name. 
		if (!Path.EndsInDirectorySeparator(path))
		{
			path += SeparatorStr;
		}
		
		var normalizedPath = Path.GetDirectoryName(path);
		if (normalizedPath is null)
		{
			return new PathValidationException("Path is invalid.");
		}
		
		// The previously called Path.GetDirectoryName() will leave 2 separators at the beginning (if there was 2 or more).
		// Even though we should not care about them as long as we use Path.Join() instead of Path.Combine(),
		// they will be reduced to a single separator for consistency.
		if (normalizedPath.StartsWith(DoubleSeparator))
		{
			normalizedPath = normalizedPath.Remove(0, 1);
		}
		
		foreach (var segment in normalizedPath.Split(Path.DirectorySeparatorChar))
		{
			if (segment.StartsWith(' ') || segment.EndsWith(' '))
			{
				return new PathValidationException($"Path segment '/{segment}/' must not start or end with a whitespace.");
			}
			
			if (segment.EndsWith('.'))
			{
				return new PathValidationException($"Path segment '/{segment}/' must not end with a period.");
			}
			
			if (segment.Any(c => char.IsControl(c)
#if _WINDOWS
				|| InvalidPrintableChars.Contains(c)))
			{
				return new PathValidationException(
					$"Path segment must not contain Control characters or any of the following symbols: {InvalidPrintableChars}");
			}
#else
			))
			{
				return new PathValidationException("Path segment must not contain Control characters.");
			}
#endif
		}
		
		return normalizedPath;
	}
}