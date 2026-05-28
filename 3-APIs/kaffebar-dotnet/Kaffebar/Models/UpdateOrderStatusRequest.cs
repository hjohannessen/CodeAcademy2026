using System.ComponentModel.DataAnnotations;

namespace Kaffebar.Models;

public class UpdateOrderStatusRequest
{
    [Required]
    [EnumDataType(
        typeof(OrderStatus),
        ErrorMessage = "Status må være PENDING, BREWING eller READY."
    )]
    public string Status { get; set; } = default!;
}
