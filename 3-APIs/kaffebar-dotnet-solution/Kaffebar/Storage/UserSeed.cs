using Kaffebar.Models;

namespace Kaffebar.Storage;

/// <summary>
/// Dummy-brukerne bak <c>GET /users</c>.
/// </summary>
/// <remarks>
/// Egen fil og ikke en del av <see cref="Seed"/>: menyen og ordrene der finnes også i
/// Java-fasiten og i mock-lageret til kaffebar-web, og må endres i takt med dem.
/// Brukerne finnes bare i .NET-sporet.
///
/// Henrik er inaktiv og ligger midt i lista. Da oppdager testene både et filter som
/// mangler og et filter som bare tar de første brukerne.
/// </remarks>
public static class UserSeed
{
    public static readonly IReadOnlyList<User> Users =
    [
        new(Guid.Parse("cafe0001-0000-4000-8000-000000000001"), "Emil", true),
        new(Guid.Parse("cafe0001-0000-4000-8000-000000000002"), "Henrik", false),
        new(Guid.Parse("cafe0001-0000-4000-8000-000000000003"), "Nora", true),
        new(Guid.Parse("cafe0001-0000-4000-8000-000000000004"), "Sigrid", true)
    ];
}
