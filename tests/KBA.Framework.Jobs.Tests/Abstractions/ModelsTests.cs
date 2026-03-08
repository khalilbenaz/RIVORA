namespace KBA.Framework.Jobs.Tests.Abstractions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Models;
using KBA.Framework.Jobs.Abstractions.Options;
using NSubstitute;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests for the JobInfo model.
/// </summary>
public class JobInfoTests
{
    [Fact]
    public void JobInfo_Initialization_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var jobInfo = new JobInfo();

        // Assert
        jobInfo.Id.Should().NotBeNullOrEmpty();
        jobInfo.Name.Should().BeEmpty();
        jobInfo.JobType.Should().BeEmpty();
        jobInfo.Status.Should().Be(JobStatus.Pending);
        jobInfo.MaxRetries.Should().Be(3);
        jobInfo.Priority.Should().Be(5);
        jobInfo.IsRecurring.Should().BeFalse();
        jobInfo.ExecutionCount.Should().Be(0);
        jobInfo.FailedCount.Should().Be(0);
        jobInfo.Metadata.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void JobInfo_Properties_ShouldBeSettable()
    {
        // Arrange
        var jobInfo = new JobInfo();
        var now = DateTime.UtcNow;

        // Act
        jobInfo.Name = "TestJob";
        jobInfo.JobType = "TestNamespace.TestJob";
        jobInfo.Status = JobStatus.Running;
        jobInfo.MaxRetries = 5;
        jobInfo.Priority = 1;
        jobInfo.IsRecurring = true;
        jobInfo.CronExpression = "0 */5 * * * *";
        jobInfo.RecurringInterval = TimeSpan.FromMinutes(5);
        jobInfo.NextExecution = now;
        jobInfo.Metadata["key"] = "value";

        // Assert
        jobInfo.Name.Should().Be("TestJob");
        jobInfo.JobType.Should().Be("TestNamespace.TestJob");
        jobInfo.Status.Should().Be(JobStatus.Running);
        jobInfo.MaxRetries.Should().Be(5);
        jobInfo.Priority.Should().Be(1);
        jobInfo.IsRecurring.Should().BeTrue();
        jobInfo.CronExpression.Should().Be("0 */5 * * * *");
        jobInfo.RecurringInterval.Should().Be(TimeSpan.FromMinutes(5));
        jobInfo.NextExecution.Should().Be(now);
        jobInfo.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }
}

/// <summary>
/// Tests for the JobSchedulerOptions model.
/// </summary>
public class JobSchedulerOptionsTests
{
    [Fact]
    public void JobSchedulerOptions_Initialization_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var options = new JobSchedulerOptions();

        // Assert
        options.JobProvider.Should().Be("Hangfire");
        options.DefaultMaxRetries.Should().Be(3);
        options.RetryDelaySeconds.Should().Be(30);
        options.UseExponentialBackoff.Should().BeTrue();
        options.DbProvider.Should().Be("SqlServer");
        options.DefaultQueue.Should().Be("default");
        options.EnableDashboard.Should().BeTrue();
        options.DashboardPath.Should().Be("/hangfire");
        options.RequireDashboardAuth.Should().BeTrue();
        options.ServerName.Should().Be("default");
        options.WorkerCount.Should().Be(10);
        options.PollingIntervalSeconds.Should().Be(1);
        options.EnablePersistence.Should().BeTrue();
        options.SchemaName.Should().Be("dbo");
        options.ProviderSettings.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void JobSchedulerOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new JobSchedulerOptions();

        // Act
        options.JobProvider = "Quartz";
        options.ConnectionString = "Server=localhost;Database=Jobs;";
        options.DefaultMaxRetries = 5;
        options.RetryDelaySeconds = 60;
        options.UseExponentialBackoff = false;
        options.EnablePersistence = false;
        options.WorkerCount = 20;

        // Assert
        options.JobProvider.Should().Be("Quartz");
        options.ConnectionString.Should().Be("Server=localhost;Database=Jobs;");
        options.DefaultMaxRetries.Should().Be(5);
        options.RetryDelaySeconds.Should().Be(60);
        options.UseExponentialBackoff.Should().BeFalse();
        options.EnablePersistence.Should().BeFalse();
        options.WorkerCount.Should().Be(20);
    }
}

/// <summary>
/// Tests for the JobStatus enum.
/// </summary>
public class JobStatusTests
{
    [Theory]
    [InlineData(JobStatus.Pending)]
    [InlineData(JobStatus.Running)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    [InlineData(JobStatus.Cancelled)]
    [InlineData(JobStatus.Scheduled)]
    [InlineData(JobStatus.WaitingForRetry)]
    public void JobStatus_Values_ShouldBeDefined(JobStatus expectedStatus)
    {
        // Act & Assert
        Enum.IsDefined(typeof(JobStatus), expectedStatus).Should().BeTrue();
    }

    [Fact]
    public void JobStatus_DefaultValue_ShouldBePending()
    {
        // Arrange & Act
        var defaultStatus = default(JobStatus);

        // Assert
        defaultStatus.Should().Be(JobStatus.Pending);
    }
}
