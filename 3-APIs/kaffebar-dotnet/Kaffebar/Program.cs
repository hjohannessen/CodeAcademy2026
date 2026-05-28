using System.Text.Json.Serialization;
using Kaffebar.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Registrer OpenAPI-tjenestene. Disse genererer spesifikasjonen
// automatisk basert på endepunktene og typene i prosjektet (code-first).
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Samme konfigurasjon for Minimal APIs (egen pipeline fra controllers).
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddValidation();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Eksponerer den genererte spesifikasjonen på /openapi/v1.json
    app.MapOpenApi();

    // Scalar gir et moderne, interaktivt UI på /scalar/v1
    // for å utforske og teste API-et.
    app.MapScalarApiReference();

    app.UseStatusCodePages();
}

// --- Hello Coffee --------------------------------------------------------
// Et minimal API-endepunkt som returnerer en hardkodet kaffemeny.
// Dette er utgangspunktet for workshopen. Bygg videre herfra!
app.MapGet(
        "/menu",
        () =>
            new[]
            {
                new Coffee(Guid.NewGuid(), "Kaffe Latte", 48.50m),
                new Coffee(Guid.NewGuid(), "Cappuccino", 45.00m),
                new Coffee(Guid.NewGuid(), "Espresso", 35.00m),
            }
    )
    .WithName("GetMenu")
    .WithSummary("Hent kaffemeny")
    .WithDescription("Returnerer en liste over alle tilgjengelige kaffedrikker i kaffebaren.");

app.MapControllers();

app.Run();

public record Coffee(Guid Id, string Name, decimal Price);
