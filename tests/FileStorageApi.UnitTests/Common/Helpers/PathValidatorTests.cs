using FileStorageApi.Common.Exceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common;
using FluentAssertions;
using Xunit;

namespace FileStorageApi.UnitTests.Common.Helpers;

public class PathValidatorTests
{
	[Fact]
	public void MustRemoveRedundantSeparators()
	{
		// Arrange
		const string path = @"\\Path//To\\Some\/\/where";
		
		// Act
		Result<string> result = PathValidator.Validate(path);
		
		// Assert
		result.State.Should().Be(ResultState.Ok);
		
		string normalizedPath = result.Value;
		
#if _WINDOWS
		normalizedPath.Should().Be(@"\Path\To\Some\where");
#else
		normalizedPath.Should().Be("/Path/To/Some/where");
#endif
	}
	
	[Fact]
	public void MustFailWhenSegmentStartsOrEndsWithWhitespace()
	{
		// Arrange
		const string segmentStartsWithWhitespace = @"Path\ To\Somewhere";
		const string segmentEndsWithWhitespace = @"Path\To \Somewhere";
		
		// Act
		Result<string> result1 = PathValidator.Validate(segmentStartsWithWhitespace);
		Result<string> result2 = PathValidator.Validate(segmentEndsWithWhitespace);
		
		// Assert
		result1.State.Should().Be(ResultState.Err);
		result1.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should().Be("Path segment '/ To/' must not start or end with a whitespace.");
		
		result2.State.Should().Be(ResultState.Err);
		result2.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should().Be("Path segment '/To /' must not start or end with a whitespace.");
	}
	
	[Fact]
	public void MustFailWhenSegmentEndsWithPeriod()
	{
		// Arrange
		const string path1 = "/Path/To./Somewhere";
		const string path2 = @"\Path\..\Somewhere";
		
		// Act
		Result<string> result1 = PathValidator.Validate(path1);
		Result<string> result2 = PathValidator.Validate(path2);
		
		// Assert
		result1.State.Should().Be(ResultState.Err);
		result1.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should().Be("Path segment '/To./' must not end with a period.");
		
		result2.State.Should().Be(ResultState.Err);
		result2.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should().Be("Path segment '/../' must not end with a period.");
	}
	
	[Theory]
	[InlineData("Path/\0To/Somewhere")]
	[InlineData("Path/T\x1Fo/Somewhere")]
	[InlineData("Path/To\0/Somewhere")]
	public void MustFailWhenSegmentContainsControlCharacters(string path)
	{
		// Arrange
		// Act
		var result = PathValidator.Validate(path);
		
		// Assert
		result.State.Should().Be(ResultState.Err);
		
		result.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should()
#if _WINDOWS
			.Be("Path segment must not contain Control characters or any of the following symbols: <>:\"|?*");
#else
			.Be("Path segment must not contain Control characters.";
#endif
	}
	
#if _WINDOWS
	[Theory]
	[InlineData("Path/To/<Somewhere")]
	[InlineData("Path/To/S>omewhere")]
	[InlineData("Path/To/So:mewhere")]
	[InlineData("Path/To/Som\"ewhere")]
	[InlineData("Path/To/Some|where")]
	[InlineData("Path/To/Somew?here")]
	[InlineData("Path/To/Somewh*ere")]
	public void MustFailWhenSegmentContainsRestrictedSymbolsOnWindows(string path)
	{
		// Arrange
		// Act
		var result = PathValidator.Validate(path);
		
		// Assert
		result.State.Should().Be(ResultState.Err);
		
		result.Error.Should().BeOfType<PathValidationException>()
			.Which.Message.Should().Be("Path segment must not contain Control characters or any of the following symbols: <>:\"|?*");
	}
#endif
}