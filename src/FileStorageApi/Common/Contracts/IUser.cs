using System;

namespace FileStorageApi.Common.Contracts;

public interface IUser
{
	Guid Id();
	Guid FolderId();
}