using System.Collections.Concurrent;
using Kaffebar.Models;

namespace Kaffebar.Storage;

/// <summary>
/// Brukerne i minne, seedet fra <see cref="UserSeed"/>.
/// </summary>
/// <remarks>
/// Samme valg som <see cref="InMemoryOrderRepository"/>: ingen database, og alt
/// nullstilles ved omstart.
/// </remarks>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public InMemoryUserRepository() => Reset();

    // Ordinal sortering, slik at rekkefølgen ikke avhenger av språkinnstillingene på
    // maskinen som kjører API-et.
    public IReadOnlyList<User> List(bool active) =>
        _users.Values
            .Where(user => user.Active == active)
            .OrderBy(user => user.Name, StringComparer.Ordinal)
            .ToList();

    public void Reset()
    {
        _users.Clear();
        foreach (var user in UserSeed.Users)
        {
            _users[user.Id] = user;
        }
    }
}
