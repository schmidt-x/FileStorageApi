namespace FileStorageApi.Options;

public class StorageOptions
{
	public const string Storage = "Storage";
	
	public string StorageFolder { get; init; } = default!;
	public long FileSizeLimit { get; set; }
}