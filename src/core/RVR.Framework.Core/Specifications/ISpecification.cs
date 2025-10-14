using System.Linq.Expressions;

namespace RVR.Framework.Core.Specifications;

/// <summary>
/// Interface pour le pattern Specification
/// </summary>
/// <typeparam name="T">Type de l'entité</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Critère de filtrage (Where)
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Liste des jointures (Include)
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Liste des jointures par chaîne de caractères (Include)
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Tri ascendant
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Tri descendant
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Nombre d'éléments à sauter (Skip)
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Nombre d'éléments à prendre (Take)
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Indique si la pagination est activée
    /// </summary>
    bool IsPagingEnabled { get; }
}
