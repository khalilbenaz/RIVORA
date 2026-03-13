using KBA.Framework.Core.Specifications;
using KBA.Framework.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Infrastructure.Data;

/// <summary>
/// Évaluateur de spécifications pour transformer une spécification en IQueryable
/// </summary>
public static class SpecificationEvaluator<TEntity, TKey> where TEntity : Entity<TKey>
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
    {
        var query = inputQuery;

        // Filtrage
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Includes typés
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // Includes par chaînes
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Tri
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Pagination
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
