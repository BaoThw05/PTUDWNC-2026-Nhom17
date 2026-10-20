using System.Linq.Expressions;

namespace CulinaryBlog.Application.Abstractions;

/// <summary>
/// Lập lịch công việc nền mà không để Application phụ thuộc Hangfire.
/// </summary>
public interface IBackgroundJobService
{
    string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall);

    string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay);

    void AddOrUpdateRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression);
}
