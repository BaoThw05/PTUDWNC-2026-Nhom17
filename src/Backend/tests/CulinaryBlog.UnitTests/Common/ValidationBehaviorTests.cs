using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Errors;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.UnitTests.Common;

public sealed class ValidationBehaviorTests
{
    private const string Response = "handled";

    [Fact]
    public async Task Handle_WithoutValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var result = await behavior.Handle(new SampleRequest(""), Next, CancellationToken.None);

        Assert.Equal(Response, result);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);

        var result = await behavior.Handle(new SampleRequest("Phở bò"), Next, CancellationToken.None);

        Assert.Equal(Response, result);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationExceptionWithErrors()
    {
        var nextCalled = false;
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);

        var exception = await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.ValidationException>(
            () => behavior.Handle(new SampleRequest(""), _ =>
            {
                nextCalled = true;
                return Task.FromResult(Response);
            }, CancellationToken.None));

        Assert.False(nextCalled);
        Assert.Equal(ErrorCodes.ValidationError, exception.Code);
        Assert.True(exception.Errors.ContainsKey(nameof(SampleRequest.Title)));
    }

    private static Task<string> Next(CancellationToken cancellationToken) => Task.FromResult(Response);

    public sealed record SampleRequest(string Title) : IRequest<string>;

    private sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(request => request.Title).NotEmpty();
        }
    }
}
