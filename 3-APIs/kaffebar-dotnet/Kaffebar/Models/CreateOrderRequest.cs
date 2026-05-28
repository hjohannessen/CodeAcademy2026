using System.ComponentModel.DataAnnotations;

namespace Kaffebar.Models;

public record CreateOrderRequest(
    [Required] Guid CoffeeId,
    [Required] OrderStatus Status,
    [Required] CoffeeSize Size,
    [Required] MilkType MilkType,
    [Required]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage = "CustomerName må være mellom 2 og 50 tegn."
    )]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "CustomerName kan ikke være blank.")]
        string CustomerName,
    [Range(1, 10, ErrorMessage = "Quantity må være mellom 1 og 10.")] int Quantity = 1,
    bool ExtraShot = false
);
