using FileStorageApi.Common.Exceptions.FileExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common;
using FluentAssertions;
using Xunit;

namespace FileStorageApi.UnitTests.Common.Helpers;

public class FilePathInfoTests
{
	[Theory]
	[InlineData("image.jpg",             "image",  ".jpg", "/")]
	[InlineData(" image .jpg  ",         "image",  ".jpg", "/")]
	[InlineData("New folder/ image.jpg", "image",  ".jpg", "/New folder")]
	[InlineData("New folder/.jpg",       "",       ".jpg", "/New folder")]
	[InlineData("New folder/image",      "image",  "",     "/New folder")]
	[InlineData("New folder/image.",     "image.", "",     "/New folder")]
	public void MustParseValidInfo(string input, string expectedName, string expectedExtension, string expectedFolderName)
	{
		// Arrange
		// Act
		var fileInfoResult = FilePathInfo.New(input);
		
		// Assert
		fileInfoResult.State.Should().Be(ResultState.Ok);
		var fileInfo = fileInfoResult.Value;
		
		fileInfo.Name.Should().Be(expectedName);
		fileInfo.Extension.Should().Be(expectedExtension);
		fileInfo.Folder.FullName.Should().Be(expectedFolderName);
	}
	
	[Theory]
	[InlineData("image.jpg", "/image.jpg")]
	[InlineData("New folder/image.jpg", "/New folder/image.jpg")]
	public void MustGetValidFullName(string input, string expectedFullName)
	{
		var fileInfoResult = FilePathInfo.New(input);
		
		fileInfoResult.State.Should().Be(ResultState.Ok);
		fileInfoResult.Value.FullName.Should().Be(expectedFullName);
	}
	
	[Theory]
	[InlineData("\x00image.jpg")]
	[InlineData("image\t.jpg")]
	[InlineData("image.jpg\x1F")]
	[InlineData("\x00")]
	public void MustFailWhenFilenameContainsControlChars(string fileName)
	{
		// Arrange
		// Act
		var fileInfoResult = FilePathInfo.New(fileName);
		
		// Assert
		fileInfoResult.State.Should().Be(ResultState.Err);
		fileInfoResult.Error.Should().BeOfType<InvalidFileNameException>()
			.Which.Message.Should().Be("InvalidFileName: FileName contains Control characters.");
	}
	
	[Fact]
	public void MustFailWhenFilenameLengthExceedsTheLimit()
	{
		// Arrange
		const int fileNameMaxLength = 255;
		var longPath = new string('a', fileNameMaxLength + 1);
		
		// Act
		var fileInfoResult = FilePathInfo.New(longPath, fileNameMaxLength: fileNameMaxLength); 
		
		// Assert
		fileInfoResult.State.Should().Be(ResultState.Err);
		fileInfoResult.Error.Should().BeOfType<FileNameTooLongException>()
			.Which.Message.Should()
			.Be($"FileNameTooLong: FileName's length exceeds the limit of {fileNameMaxLength} characters.");
	}
	
	[Theory]
	[InlineData("/New Folder/image.jpg", "/New Folder")]
	[InlineData("/image.jpg", "/")]
	[InlineData(@"\image.jpg", "/")]
	[InlineData("image.jpg", "/")]
	public void MustReturnValidFolderFullName(string fileName, string expectedFolderFullName)
	{
		// Arrange
		// Act
		var fileInfoResult = FilePathInfo.New(fileName);
		
		// Assert
		fileInfoResult.State.Should().Be(ResultState.Ok);
		fileInfoResult.Value.Folder.FullName.Should().Be(expectedFolderFullName);
	}
	
	[Fact]
	public void MustFailWhenFilenameIsEmpty()
	{
		// Arrange
		// Act
		var fileInfoResult = FilePathInfo.New("/New folder/");
		
		// Assert
		fileInfoResult.State.Should().Be(ResultState.Err);
		
		fileInfoResult.Error.Should().BeOfType<EmptyFileNameException>()
			.Which.Message.Should().Be("EmptyFileName: FileName is empty.");
	}
}