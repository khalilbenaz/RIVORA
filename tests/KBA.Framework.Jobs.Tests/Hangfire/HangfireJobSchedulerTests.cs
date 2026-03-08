namespace KBA.Framework.Jobs.Tests.Hangfire;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Models;
using KBA.Framework.Jobs.Abstractions.Options;
using KBA.Framework.Jobs.Hangfire.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using FluentAssertions;

/// <summary>
/// Test job implementation for unit tests.
/// </summary>
public class TestJob : IJob
{
    public int ExecutionCount { get; private set; }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ExecutionCount++;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test recurring job implementation for unit tests.
/// </summary>
public class TestRecurringJob : IRecurringJob
{
    public string CronExpression => "0 */5 * * * *";
    public string TimeZoneId => "UTC";

    public int ExecutionCount { get; private set; }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ExecutionCount++;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Tests for the HangfireJobScheduler implementation.
/// </summary>
public class HangfireJobSchedulerTests
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IOptions<JobSchedulerOptions> _options;
    private readonly ILogger<HangfireJobScheduler> _logger;
    private readonly HangfireJobScheduler _scheduler;

    public HangfireJobSchedulerTests()
    {
        _backgroundJobClient = Substitute.For<IBackgroundJobClient>();
        _recurringJobManager = Substitute.For<IRecurringJobManager>();
        _options = Options.Create(new JobSchedulerOptions
        {
            JobProvider = "Hangfire",
            DefaultMaxRetries = 3,
            DefaultQueue = "default"
        });
        _logger = Substitute.For<ILogger<HangfireJobScheduler>>();
        
        _scheduler = new HangfireJobScheduler(_backgroundJobClient, _recurringJobManager, _options, _logger);
    }

    [Fact]
    public void ProviderName_ShouldBeHangfire()
    {
        // Act
        var providerName = _scheduler.ProviderName;

        // Assert
        providerName.Should().Be("Hangfire");
    }

    [Fact]
    public async Task EnqueueAsync_WithoutParameters_ShouldReturnJobInfo()
    {
        // Arrange
        _backgroundJobClient.Enqueue<TestJob>(Arg.Any<Func<Task>>())
            .Returns("test-job-id");

        // Act
        var jobInfo = await _scheduler.EnqueueAsync<TestJob>();

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Id.Should().Be("test-job-id");
        jobInfo.Name.Should().Be("TestJob");
        jobInfo.JobType.Should().Be(typeof(TestJob).FullName);
        jobInfo.Status.Should().Be(JobStatus.Pending);
        jobInfo.MaxRetries.Should().Be(3);
        jobInfo.Queue.Should().Be("default");
    }

    [Fact]
    public async Task EnqueueAsync_WithParameters_ShouldReturnJobInfoWithData()
    {
        // Arrange
        var parameters = new Dictionary<string, string> { { "key", "value" } };
        _backgroundJobClient.Enqueue<TestJob>(Arg.Any<Func<Task>>())
            .Returns("test-job-id");

        // Act
        var jobInfo = await _scheduler.EnqueueAsync<TestJob>(parameters);

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Id.Should().Be("test-job-id");
        jobInfo.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterRecurringAsync_ShouldRegisterJob()
    {
        // Arrange
        var cronExpression = "0 */5 * * * *";
        var recurringJobId = "recurring-test";

        // Act
        var jobInfo = await _scheduler.RegisterRecurringAsync<TestRecurringJob>(
            recurringJobId, cronExpression);

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Id.Should().Be(recurringJobId);
        jobInfo.CronExpression.Should().Be(cronExpression);
        jobInfo.IsRecurring.Should().BeTrue();
        jobInfo.Status.Should().Be(JobStatus.Scheduled);

        await _recurringJobManager.Received(1).AddOrUpdateAsync<TestRecurringJob>(
            Arg.Any<string>(),
            Arg.Any<Func<Task>>(),
            Arg.Any<string>(),
            Arg.Any<TimeZoneInfo>());
    }

    [Fact]
    public async Task RegisterRecurringAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _scheduler.RegisterRecurringAsync<TestRecurringJob>("", "0 */5 * * * *"));
    }

    [Fact]
    public async Task RegisterRecurringAsync_WithEmptyCron_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _scheduler.RegisterRecurringAsync<TestRecurringJob>("test-id", ""));
    }

    [Fact]
    public async Task RemoveRecurringAsync_ShouldRemoveJob()
    {
        // Act
        var result = await _scheduler.RemoveRecurringAsync("test-id");

        // Assert
        result.Should().BeTrue();
        await _recurringJobManager.Received(1).RemoveAsync("test-id");
    }

    [Fact]
    public async Task CancelAsync_ShouldDeleteJob()
    {
        // Act
        var result = await _scheduler.CancelAsync("test-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteJob()
    {
        // Act
        var result = await _scheduler.DeleteAsync("test-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        _backgroundJobClient.GetJobCount("Enqueued").Returns(5);
        _backgroundJobClient.GetJobCount("Processing").Returns(2);
        _backgroundJobClient.GetJobCount("Scheduled").Returns(3);
        _backgroundJobClient.GetJobCount("Failed").Returns(1);
        _backgroundJobClient.GetJobCount("Succeeded").Returns(10);
        _recurringJobManager.GetRecurringJobs().Returns(new List<RecurringJobDto>());

        // Act
        var statistics = await _scheduler.GetStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.Pending.Should().Be(5);
        statistics.Processing.Should().Be(2);
        statistics.Scheduled.Should().Be(3);
        statistics.Failed.Should().Be(1);
        statistics.Succeeded.Should().Be(10);
        statistics.Total.Should().Be(21);
    }
}
