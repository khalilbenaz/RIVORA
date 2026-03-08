namespace KBA.Framework.Jobs.Tests.Quartz;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Models;
using KBA.Framework.Jobs.Abstractions.Options;
using KBA.Framework.Jobs.Quartz.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Quartz;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests for the QuartzJobScheduler implementation.
/// </summary>
public class QuartzJobSchedulerTests
{
    private readonly IScheduler _scheduler;
    private readonly IOptions<JobSchedulerOptions> _options;
    private readonly ILogger<QuartzJobScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly QuartzJobScheduler _jobScheduler;

    public QuartzJobSchedulerTests()
    {
        _scheduler = Substitute.For<IScheduler>();
        _options = Options.Create(new JobSchedulerOptions
        {
            JobProvider = "Quartz",
            DefaultMaxRetries = 3,
            DefaultQueue = "default"
        });
        _logger = Substitute.For<ILogger<QuartzJobScheduler>>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        
        _jobScheduler = new QuartzJobScheduler(_scheduler, _options, _logger, _serviceProvider);
    }

    [Fact]
    public void ProviderName_ShouldBeQuartz()
    {
        // Act
        var providerName = _jobScheduler.ProviderName;

        // Assert
        providerName.Should().Be("Quartz");
    }

    [Fact]
    public async Task EnqueueAsync_WithoutParameters_ShouldReturnJobInfo()
    {
        // Arrange
        _scheduler.ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var jobInfo = await _jobScheduler.EnqueueAsync<TestJob>();

        // Assert
        jobInfo.Should().NotBeNull();
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
        _scheduler.ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var jobInfo = await _jobScheduler.EnqueueAsync<TestJob>(parameters);

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ScheduleAsync_ShouldScheduleJob()
    {
        // Arrange
        var executeAt = DateTimeOffset.UtcNow.AddHours(1);
        _scheduler.ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var jobInfo = await _jobScheduler.ScheduleAsync<TestJob>(executeAt);

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Status.Should().Be(JobStatus.Scheduled);
        jobInfo.NextExecution.Should().Be(executeAt.UtcDateTime);
    }

    [Fact]
    public async Task RegisterRecurringAsync_ShouldRegisterJob()
    {
        // Arrange
        var cronExpression = "0 */5 * * * *";
        var recurringJobId = "recurring-test";
        _scheduler.CheckExists(Arg.Any<JobKey>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _scheduler.ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var jobInfo = await _jobScheduler.RegisterRecurringAsync<TestRecurringJob>(
            recurringJobId, cronExpression);

        // Assert
        jobInfo.Should().NotBeNull();
        jobInfo.Id.Should().Be(recurringJobId);
        jobInfo.CronExpression.Should().Be(cronExpression);
        jobInfo.IsRecurring.Should().BeTrue();
        jobInfo.Status.Should().Be(JobStatus.Scheduled);
    }

    [Fact]
    public async Task RegisterRecurringAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _jobScheduler.RegisterRecurringAsync<TestRecurringJob>("", "0 */5 * * * *"));
    }

    [Fact]
    public async Task RegisterRecurringAsync_WithEmptyCron_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _jobScheduler.RegisterRecurringAsync<TestRecurringJob>("test-id", ""));
    }

    [Fact]
    public async Task RemoveRecurringAsync_ShouldDeleteJob()
    {
        // Arrange
        _scheduler.DeleteJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _jobScheduler.RemoveRecurringAsync("test-id");

        // Assert
        result.Should().BeTrue();
        await _scheduler.Received(1).DeleteJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_ShouldInterruptJob()
    {
        // Arrange
        _scheduler.Interrupt(Arg.Any<JobKey>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _jobScheduler.CancelAsync("test-id");

        // Assert
        result.Should().BeTrue();
        await _scheduler.Received(1).Interrupt(Arg.Any<JobKey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteJob()
    {
        // Arrange
        _scheduler.DeleteJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _jobScheduler.DeleteAsync("test-id");

        // Assert
        result.Should().BeTrue();
        await _scheduler.Received(1).DeleteJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        var metaData = Substitute.For<ISchedulerMetaData>();
        metaData.CurrentlyExecutingJobs.Returns(new List<IJobExecutionContext>());
        _scheduler.GetMetaData().Returns(metaData);
        _scheduler.GetJobKeys(Arg.Any<GroupMatcher<JobKey>>(), Arg.Any<CancellationToken>())
            .Returns(new List<JobKey>());

        // Act
        var statistics = await _jobScheduler.GetStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
    }
}

/// <summary>
/// Test job implementation for Quartz tests.
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
/// Test recurring job implementation for Quartz tests.
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
