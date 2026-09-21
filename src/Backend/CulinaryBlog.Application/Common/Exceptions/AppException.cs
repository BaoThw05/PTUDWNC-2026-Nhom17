namespace CulinaryBlog.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected AppException(string code, int statusCode, string message) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string code, string message)
        : base(code, 404, message) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string code, string message)
        : base(code, 403, message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string code, string message)
        : base(code, 409, message) { }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string code, string message)
        : base(code, 422, message) { }
}