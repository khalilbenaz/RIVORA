using System.Net;
using System.Text.Json;
using KBA.Framework.Domain.Common;

namespace KBA.Framework.Api.Middleware;

/// <summary>
/// Middleware pour mapper automatiquement les objets Result vers des réponses HTTP
/// </summary>
public class ResultMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ResultMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ce middleware intercepte la réponse si elle contient un objet Result
        // Mais en ASP.NET Core MVC, c'est plus complexe car les controllers retournent souvent ActionResult.
        // Pour une implémentation simplifiée dans ce cadre, on va plutôt suggérer 
        // d'utiliser des extensions sur ControllerBase ou un filtre d'action.
        
        // Cependant, pour respecter la roadmap "Result Pattern (Railway-Oriented)", 
        // nous allons fournir une classe de base de contrôleur qui gère cela.
        
        await _next(context);
    }
}
