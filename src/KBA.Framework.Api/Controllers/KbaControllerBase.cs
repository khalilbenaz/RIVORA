using KBA.Framework.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace KBA.Framework.Api.Controllers;

/// <summary>
/// Classe de base pour les contrôleurs du framework KBA, intégrant le Result Pattern.
/// </summary>
public abstract class KbaControllerBase : ControllerBase
{
    /// <summary>
    /// Convertit un objet Result en ActionResult HTTP approprié.
    /// </summary>
    protected ActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return MapErrorResult(result);
    }

    /// <summary>
    /// Convertit un objet Result<typeparamref name="T"/> en ActionResult HTTP approprié.
    /// </summary>
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapErrorResult(result);
    }

    private ActionResult MapErrorResult(Result result)
    {
        return result.ErrorCode switch
        {
            "NotFound" => NotFound(new { message = result.Error }),
            "Unauthorized" => Unauthorized(new { message = result.Error }),
            "Forbidden" => Forbid(),
            "Conflict" => Conflict(new { message = result.Error }),
            _ => BadRequest(new { message = result.Error })
        };
    }
}
