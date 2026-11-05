using System.Globalization;
using System.Linq.Expressions;
using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class FakeBackgroundJobService : IBackgroundJobService
{
    public List<LambdaExpression> Enqueued { get; } = [];

    public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
    {
        Enqueued.Add(methodCall);
        return Enqueued.Count.ToString(CultureInfo.InvariantCulture);
    }

    public string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay) =>
        throw new NotSupportedException();

    public void AddOrUpdateRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression) =>
        throw new NotSupportedException();
}
