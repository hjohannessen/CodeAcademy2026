using Kaffebar.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kaffebar.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Opprett en ny bestilling. Returnerer 201 Created med Location-header
    /// som peker på den nye ordren.
    /// </summary>
    [HttpPost(Name = "CreateOrder")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new OrderResponse(
            OrderId: Guid.NewGuid(),
            CoffeeId: request.CoffeeId,
            CustomerName: request.CustomerName,
            Quantity: request.Quantity,
            Size: request.Size,
            MilkType: request.MilkType,
            ExtraShot: request.ExtraShot,
            CreatedAt: DateTimeOffset.UtcNow
        );

        // CreatedAtAction krever et tilsvarende GET-endepunkt for å bygge Location-headeren.
        // Vi har ikke laget GET /orders/{id} ennå (kommer i Oppgave 5), så vi bygger URI-en manuelt.
        return Created($"/orders/{order.OrderId}", order);
    }
}