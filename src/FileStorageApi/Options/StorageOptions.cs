namespace FileStorageApi.Options;

public class StorageOptions
{
	public const string Storage = "Storage";
	
	public string StorageFolder { get; init; } = default!;
	public long StorageSizeLimitPerUser { get; init; }
	public long FileSizeLimit { get; init; }
	
	public int PathSegmentMaxLength { get; init; }
	public int FileNameMaxLength { get; init; }
	public int FullPathMaxLength { get; init; }
}