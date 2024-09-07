using System;
using FileStorageApi.Domain.Enums;

namespace FileStorageApi.Domain.Models;

public record Item(string Name, long Size, ItemType Type, DateTimeOffset CreatedAt, DateTimeOffset ModifiedAt);