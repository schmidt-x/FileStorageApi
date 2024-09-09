using FileStorageApi.Domain.Enums;
using System;

namespace FileStorageApi.Data.Extensions;

public static class ItemOrderExtensions
{
	public static string ToDbName(this ItemOrder item)
	{
		return item switch
		{
			ItemOrder.Type => "type",
			ItemOrder.Name => "name",
			ItemOrder.DateCreated => "created_at",
			ItemOrder.DateModified => "modified_at",
			ItemOrder.Size => "size",
			_ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
		};
	}
}