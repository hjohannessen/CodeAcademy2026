namespace Kaffebar.Models;

public record OrderResponse(
    Guid OrderId,
    Guid CoffeeId,
    string CustomerName,
    int Quantity,
    CoffeeSize Size,
    MilkType MilkType,
    bool ExtraShot,
    DateTimeOffset CreatedAt
);