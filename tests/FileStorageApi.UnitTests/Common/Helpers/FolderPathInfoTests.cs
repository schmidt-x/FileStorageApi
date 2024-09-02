using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common;
using FluentAssertions;
using Xunit;

namespace FileStorageApi.UnitTests.Common.Helpers;

public class FolderPathInfoTests
{
	[Theory]
	[InlineData(@"/\FolderA \/FolderB  / / FolderC \\", "/FolderA/FolderB/", "FolderC")]
	[InlineData(@"/FolderA\", "/", "FolderA")]
	[InlineData("/", "", "/")]
	[InlineData(" / ", "", "/")]
	public void MustNormalizePath(string folderPath, string expectedPath, string expectedName)
	{
		// Arrange
		// Act
		var res = FolderPathInfo.New(folderPath);
		
		// Assert
		res.State.Should().Be(ResultState.Ok);
		var pathInfo = res.Value;
		
		pathInfo.Path.Should().Be(expectedPath);
		pathInfo.Name.Should().Be(expectedName);
	}
	
	[Theory]
	[InlineData("/FolderA/F\tolderB/")]
	[InlineData("/FolderA/Fol\rderB/")]
	[InlineData("/FolderA/Folder\nB/")]
	[InlineData("/FolderA/FolderB/\x1F")]
	[InlineData("/\x00/")]
	public void MustFailWhenSegmentContainsControlChars(string path)
	{
		// Arrange
		const ResultState expectedState = ResultState.Err;
		const string expectedErrorMessage = "InvalidPath: FolderName contains Control characters.";
		
		// Act
		var res = FolderPathInfo.New(path);
		
		// Assert
		res.State.Should().Be(expectedState);
		res.Error.Should().BeOfType<InvalidPathException>()
			.Which.Message.Should().Be(expectedErrorMessage);
	}
	
	[Fact]
	public void MustReturnValidParent()
	{
		// Arrange
		var pathInfo = FolderPathInfo.New("/FolderA/FolderB").Value;
		
		// Act and Assert
		// parent1
		var parent1Exists = pathInfo.TryGetParent(out var parent1);
		
		parent1Exists.Should().Be(true);
		parent1.Should().NotBeNull();
		parent1!.Path.Should().Be("/");
		parent1.Name.Should().Be("FolderA");
		parent1.IsRootFolder.Should().Be(false);
		
		// parent2 (root folder)
		var parent2Exists = parent1.TryGetParent(out var parent2);
		
		parent2Exists.Should().Be(true);
		parent2.Should().NotBeNull();
		parent2!.Path.Should().Be("");
		parent2.Name.Should().Be("/");
		parent2.IsRootFolder.Should().Be(true);
		
		// parent3
		var parent3Exists = parent2.TryGetParent(out var parent3);
		
		parent3Exists.Should().Be(false);
		parent3.Should().BeNull();
	}
	
	[Fact]
	public void MustFailWhenSegmentIsTooLong()
	{
		// Arrange
		const int pathSegmentMaxLength = 256;
		var longPath = new string('a', pathSegmentMaxLength + 1);
		
		// Act
		var folderInfoResult = FolderPathInfo.New(longPath, pathSegmentMaxLength);
		
		// Assert
		folderInfoResult.State.Should().Be(ResultState.Err);
		folderInfoResult.Error.Should().BeOfType<FolderNameTooLongException>()
			.Which.Message.Should()
			.Be($"FolderNameTooLong: FolderName's length exceeds the limit of {pathSegmentMaxLength} characters.");
	}
	
	[Theory]
	[InlineData("/", true)]
	[InlineData(@"\", true)]
	[InlineData("//", true)]
	[InlineData("Hello", false)]
	[InlineData("/New folder", false)]
	public void MustBeOrNotBeRootFolder(string folderName, bool isRoot)
	{
		// Arrange
		// Act
		var folder = FolderPathInfo.New(folderName).Value;
		
		// Assert
		folder.IsRootFolder.Should().Be(isRoot);
	}
}