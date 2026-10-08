using System.Linq.Expressions;
using CulinaryBlog.Application.Abstractions;
using Hangfire;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

internal sealed class HangfireBackgroundJobService(
    IBackgroundJobClient backgroundJobs,
    IRecurringJobManager recurringJobs) : IBackgroundJobService
{
    public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall) =>
        backgroundJobs.Enqueue(methodCall);

    public string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay) =>
        backgroundJobs.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        Expression<Func<TJob, Task>> methodCall,
        string cronExpression) =>
        recurringJobs.AddOrUpdate(recurringJobId, methodCall, cronExpression);
}
