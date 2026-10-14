namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class FileSizeExceededException(string message, string code) : AppException(code, message);

public sealed class FileTypeNotAllowedException(string message, string code) : AppException(code, message);

public sealed class StorageUnavailableException(string message) : AppException("STORAGE_UNAVAILABLE", message);
