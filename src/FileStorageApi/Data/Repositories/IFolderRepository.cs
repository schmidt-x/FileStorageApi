using System.Threading;
using System.Threading.Tasks;
using FileStorageApi.Domain.Entities;

namespace FileStorageApi.Data.Repositories;

public interface IFolderRepository
{
	Task CreateFolder(Folder folder, CancellationToken ct);
}