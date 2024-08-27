﻿using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace FileStorageApi.Data.Repositories;

public interface IFileRepository
{
	Task CreateAsync(File file, CancellationToken ct);
	Task<bool> ExistsAsync(string fileName, string fileExtension, Guid folderId, Guid userId, CancellationToken ct);
}