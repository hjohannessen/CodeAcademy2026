using System.Collections.Concurrent;
using Kaffebar.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Kaffebar.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, OrderResponse> _orders = new();

    [HttpPost(Name = "CreateOrder")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Results<CreatedAtRoute<OrderResponse>, BadRequest<ValidationProblemDetails>> CreateOrder(
        [FromBody] CreateOrderRequest request
    )
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

        _orders[order.OrderId] = order;

        return TypedResults.CreatedAtRoute(order, "GetOrder", new { orderId = order.OrderId });
    }

    [HttpGet("{orderId:guid}", Name = "GetOrder")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Results<Ok<OrderResponse>, NotFound> GetOrder(Guid orderId)
    {
        return _orders.TryGetValue(orderId, out var order)
            ? TypedResults.Ok(order)
            : TypedResults.NotFound();
    }

    [HttpPatch("{orderId:guid}", Name = "UpdateOrderStatus")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Results<
        Ok<OrderResponse>,
        BadRequest<ValidationProblemDetails>,
        NotFound
    > UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!_orders.TryGetValue(orderId, out var order))
        {
            return TypedResults.NotFound();
        }
        var updatedOrder = order with { Status = Enum.Parse<OrderStatus>(request.Status) };
        _orders[orderId] = updatedOrder;
        return TypedResults.Ok(updatedOrder);
    }
}
