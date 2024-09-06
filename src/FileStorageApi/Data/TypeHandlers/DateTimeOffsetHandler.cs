using System.Data;
using System;
using Dapper;

namespace FileStorageApi.Data.TypeHandlers;

public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
	public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
		=> throw new NotImplementedException("This method is not called");
	
	public override DateTimeOffset Parse(object value) => (DateTime)value;
}