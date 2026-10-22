using Kaffebar.Models;

namespace Kaffebar.Storage;

/// <summary>
/// Lagringen av brukere, bak et lite grensesnitt av samme grunn som
/// <see cref="IOrderRepository"/>.
/// </summary>
public interface IUserRepository
{
    /// <summary>Brukerne som er aktive eller inaktive, sortert på navn.</summary>
    IReadOnlyList<User> List(bool active);

    /// <summary>Tømmer og seeder på nytt. Brukes av testene.</summary>
    void Reset();
}
