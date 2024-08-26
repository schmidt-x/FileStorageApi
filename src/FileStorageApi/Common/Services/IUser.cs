using System;

namespace FileStorageApi.Common.Services;

public interface IUser
{
	Guid Id();
	Guid FolderId();
}