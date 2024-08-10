namespace FileStorageApi.Options;

public class StorageOptions
{
	public const string Storage = "Storage";
	
	public string RootFolder { get; init; } = default!;
	public string StorageFolder { get; init; } = default!;
	public string TrashFolder { get; init; } = default!;
}