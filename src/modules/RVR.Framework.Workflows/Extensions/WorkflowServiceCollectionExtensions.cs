namespace RVR.Framework.Workflows.Extensions;

using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Workflows.Abstractions;
using RVR.Framework.Workflows.Engine;
using RVR.Framework.Workflows.Stores;

/// <summary>
/// Extension methods for registering workflow services.
/// </summary>
public static class WorkflowServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RVR lightweight workflow engine with the in-memory store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrWorkflows(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowStore, InMemoryWorkflowStore>();
        services.AddScoped<WorkflowEngine>();
        return services;
    }

    /// <summary>
    /// Adds the RVR lightweight workflow engine with a custom store implementation.
    /// </summary>
    /// <typeparam name="TStore">The store implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrWorkflows<TStore>(this IServiceCollection services)
        where TStore : class, IWorkflowStore
    {
        services.AddSingleton<IWorkflowStore, TStore>();
        services.AddScoped<WorkflowEngine>();
        return services;
    }

    /// <summary>
    /// Registers a workflow definition in the dependency injection container.
    /// </summary>
    /// <typeparam name="TWorkflow">The workflow implementation type.</typeparam>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWorkflow<TWorkflow, TState>(this IServiceCollection services)
        where TWorkflow : class, IWorkflow<TState>
        where TState : notnull
    {
        services.AddSingleton<IWorkflow<TState>, TWorkflow>();
        return services;
    }
}
