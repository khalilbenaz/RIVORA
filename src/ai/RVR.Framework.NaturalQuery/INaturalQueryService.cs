namespace RVR.Framework.NaturalQuery;

using RVR.Framework.NaturalQuery.Models;

/// <summary>
/// Service for converting natural language queries into LINQ expressions.
/// </summary>
public interface INaturalQueryService
{
    /// <summary>
    /// Parses a natural language query into a structured QueryPlan.
    /// </summary>
    /// <param name="naturalLanguageQuery">The natural language query string.</param>
    /// <param name="entityType">The target entity type to query against.</param>
    /// <returns>A structured <see cref="QueryPlan"/> representing the parsed query.</returns>
    QueryPlan Parse(string naturalLanguageQuery, Type entityType);

    /// <summary>
    /// Applies a QueryPlan to an IQueryable source.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="plan">The query plan to apply.</param>
    /// <returns>The queryable with filters, sorts, and pagination applied.</returns>
    IQueryable<T> Apply<T>(IQueryable<T> source, QueryPlan plan) where T : class;

    /// <summary>
    /// Convenience method: parse + apply in one call.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="naturalLanguageQuery">The natural language query string.</param>
    /// <returns>The queryable with the parsed query applied.</returns>
    IQueryable<T> Query<T>(IQueryable<T> source, string naturalLanguageQuery) where T : class;
}
