using Kaffebar.Models;
using Kaffebar.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Kaffebar.Controllers;

/// <summary>
/// Brukere. Foreløpig bare dummy-data, og ikke en del av oppgavesettet.
/// </summary>
/// <remarks>
/// Finnes bare i .NET-sporet. Java-kontrakten (kaffebar-api.yaml) har ingen
/// <c>/users</c>, så her er sporene bevisst ulike. Skal frontenden bruke endepunktet,
/// må det inn i kontrakten først.
///
/// En controller og ikke et Minimal API, fordi <c>active</c> er input som kan gi 400,
/// og bare controllerne får automatisk validering. Se FASIT.md, oppgave 2.
/// </remarks>
[ApiController]
[Route("users")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
public class UsersController(IUserRepository repository) : ControllerBase
{
    /// <summary>Hent brukere.</summary>
    /// <remarks>
    /// <c>active</c> er <c>true</c> som standard, så <c>GET /users</c> gir det samme som
    /// <c>GET /users?active=true</c>. De inaktive brukerne må man be om.
    /// </remarks>
    [HttpGet]
    [EndpointSummary("Hent brukere")]
    [EndpointDescription("Returnerer brukerne sortert på navn. Uten active gir den bare de aktive brukerne, og active=false gir de inaktive.")]
    [ProducesResponseType<IEnumerable<User>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<User>> ListUsers([FromQuery] bool active = true)
    {
        return Ok(repository.List(active));
    }
}
