using System;

namespace FileStorageApi.Domain.Entities;

public record FolderPath(Guid Id, string Path, Guid UserId);