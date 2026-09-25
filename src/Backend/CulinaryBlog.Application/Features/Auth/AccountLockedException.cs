using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>Tài khoản đang bị khóa tạm thời vì đăng nhập sai nhiều lần, trả về HTTP 423.</summary>
public sealed class AccountLockedException()
    : AppException(AuthErrorCodes.AccountLocked, "The account is temporarily locked.");
