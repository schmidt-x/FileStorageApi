using System.Collections.Generic;
using System;

namespace FileStorageApi.Common.Models;

public class PaginatedList<T>
{
	public int PageNumber { get; }
	public int TotalPages { get; }
	public int TotalCount { get; }
	
	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;
	
	public IReadOnlyCollection<T> Items { get; }
	
	public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
	{
		PageNumber = pageNumber;
		TotalPages = pageSize == 0 ? count : (int)Math.Ceiling(count / (double)pageSize);
		TotalCount = count;
		Items = items;
	}
}