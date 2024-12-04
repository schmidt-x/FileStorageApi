using static FileStorageApi.Domain.Constants.ErrorCodes;

namespace FileStorageApi.Common.Models;

public static class ErrorDetails
{
	public static readonly ErrorDetail AuthFailure = new (AUTH_FAILURE, string.Empty, "Login or Password is incorrect.");
}