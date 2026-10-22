namespace Kaffebar.Models;

/// <summary>En bruker av kaffebaren. Foreløpig bare dummy-data.</summary>
/// <param name="Id">Fast uuid, slik at id-en er den samme ved hver oppstart.</param>
/// <param name="Name">Navnet på brukeren.</param>
/// <param name="Active">Om brukeren er aktiv. <c>GET /users</c> gir bare aktive brukere som standard.</param>
public record User(Guid Id, string Name, bool Active);
