---
name: nytt-endepunkt
description: 'Oppskrift for å lage et nytt .NET (dotnet) REST-endepunkt i Kaffebar-API-et i 3-APIs/kaffebar-dotnet-solution, etter mønsteret i repoet: controller eller Minimal API, egne request- og response-records med validering, lagring i minne, feilsvar som RFC 7807 application/problem+json via ProblemFactory, OpenAPI-metadata, kall i Kaffebar.http og WebApplicationFactory-tester. Bruk når du skal legge til, utvide eller endre en rute i ASP.NET Core-API-et, for eksempel et nytt /orders-endepunkt (GET, POST, PATCH, DELETE) eller en ny ressurs med egen controller. Ikke for Java-sporet (contract-first) eller starteren i 3-APIs/kaffebar-dotnet.'
argument-hint: 'Metode, sti og formål, f.eks. «DELETE /orders/{orderId} som sletter en bestilling»'
---

# Nytt REST-endepunkt i Kaffebar-API-et (.NET)

Slik lager du et endepunkt i [`3-APIs/kaffebar-dotnet-solution`](../../../3-APIs/kaffebar-dotnet-solution/) som ser ut og oppfører seg som endepunktene som finnes: ordre-endepunktene i `OrdersController` og Minimal API-ene `GET /menu` og `GET /health`.

API-et er code-first, så OpenAPI-spesifikasjonen utledes av koden. Den skal likevel stemme med Java-sporets kontrakt: samme stier, feltnavn (camelCase), enum-verdier, statuskoder og media types. [`FASIT.md`](../../../3-APIs/kaffebar-dotnet-solution/FASIT.md) begrunner valgene under, oppgave for oppgave.

## Når skal den brukes

- Et nytt endepunkt på en ressurs som finnes, for eksempel under `/orders/{orderId}`.
- En ny ressurs med egen controller.
- En endring i metode, sti, parametere eller svar på et endepunkt som finnes.

Den gjelder ikke starteren i `3-APIs/kaffebar-dotnet/`, som er deltakernes utgangspunkt og ikke har testprosjekt, eller `Oppgave9/`, som er et frittstående eksperiment. Den gjelder heller ikke Java-sporet. Der endrer du `kaffebar-api.yaml` og implementerer det genererte grensesnittet.

## Filene

Stiene er relative til `3-APIs/kaffebar-dotnet-solution/`.

| Fil | Rolle |
| --- | --- |
| [`Kaffebar/Controllers/OrdersController.cs`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Controllers/OrdersController.cs) | Mønsteret for controller-endepunkter |
| [`Kaffebar/Program.cs`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Program.cs) | Oppsett, JSON-innstillinger og Minimal API-ene |
| [`Kaffebar/Models/`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Models/) | Request- og response-records og enums |
| [`Kaffebar/Storage/`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Storage/) | `IOrderRepository`, `InMemoryOrderRepository` og `Seed` |
| [`Kaffebar/Errors/ProblemFactory.cs`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Errors/ProblemFactory.cs) | Alle feilsvar (RFC 7807) |
| [`Kaffebar/OpenApi/`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/OpenApi/) | Transformerne som retter den utledede spesifikasjonen |
| [`Kaffebar/Kaffebar.http`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar/Kaffebar.http) | Kall mot alle endepunktene, inkludert feiltilfellene |
| [`Kaffebar.Tests/KaffebarApiTests.cs`](../../../3-APIs/kaffebar-dotnet-solution/Kaffebar.Tests/KaffebarApiTests.cs) | Testene, med `WebApplicationFactory<Program>` |

## Fremgangsmåte

1. **Bestem kontrakten før du skriver kode.** Skriv ned metode, sti, request-body, route- og query-parametere og alle svarene, også feilene og statuskodene deres.
   - Finnes endepunktet allerede i Java-kontrakten (`3-APIs/kaffebar-java-solution/src/main/resources/api/kaffebar-api.yaml`), er den fasit.
   - Velg metode slik FASIT gjør i oppgave 7: `PATCH` endrer ett felt på en ressurs, og `POST /ressurs/{id}/handling` er en handling med sideeffekter. Ikke bruk `PUT` til delvise endringer.
   - En ny ressurs gir `201 Created` med `Location`-header.

2. **Velg stil.** Begge stilene finnes i prosjektet med vilje (oppgave 2), så ikke flytt et endepunkt som finnes fra den ene stilen til den andre.
   - **Controller er standard.** Endepunkter som tar imot en body eller parametere, eller som kan svare med en feil, hører hjemme i en controller. Nye `/orders`-endepunkter legges i `OrdersController`, og en ny ressurs får sin egen controller i `Controllers/`.
   - **Minimal API er bare for små lese-endepunkter uten input og uten feilsvar**, som `/menu` og `/health`. Her får bare controllerne automatisk validering, fordi `AddValidation()` ikke er registrert. `ProblemFactory…AsResult()` lager dessuten et MVC-`ObjectResult`, som bare controllere kan returnere.

3. **Lag modellene i `Kaffebar/Models/`.** Bruk namespace `Kaffebar.Models`, og skriv XML-dokumentasjonen på norsk.
   - Lag en egen request-record for hver operasjon, slik `CreateOrderRequest` og `UpdateOrderStatusRequest` er. Ikke bruk response-typen (`Order`) som input.
   - Gi en request med valideringsregler `init`-egenskaper, ikke posisjonelle parametere. `[property: …]` på en posisjonell record gir 500 ved første kall, og attributter på konstruktør-parametere forsvinner fra spesifikasjonen. En posisjonell record holder bare når feltene ikke trenger mer enn `[Required]`, som i `UpdateOrderStatusRequest`.
   - Merk påkrevde felt med `required` og `[Required]`, og gjør valgfrie felt nullable (`MilkType?`). En default-verdi får både en initialiserer og `[DefaultValue]`. Feilmeldingene skrives på norsk med feltnavnet i camelCase, for eksempel `ErrorMessage = "quantity må være mellom 1 og 10."`.
   - Lag response-typer som posisjonelle records der de valgfrie, nullable feltene står **sist** med `= null`. Ellers havner de i `required`-lista i spesifikasjonen.
   - Gi enum-medlemmer PascalCase-navn og `[JsonStringEnumMemberName("…")]` med verdien kontrakten bruker, for eksempel `PENDING` i ordre-enumene og `ok` i helsesjekken.

4. **Utvid lagringen.**
   - Legg metoden i `IOrderRepository` med en `<summary>`, og implementer den i `InMemoryOrderRepository`. Den bruker `ConcurrentDictionary`, endrer records med `with` og lager tidsstempler med `Now()` (UTC, millisekunder).
   - Returner `null` når noe ikke finnes, så bestemmer controlleren statuskoden.
   - En ny ressurs kan få et eget repository-grensesnitt med en implementasjon i minne. Registrer det som singleton under `// --- Lagring ---` i `Program.cs`.
   - Ny tilstand må nullstilles i `Reset()`, fordi testene kaller den.
   - Endrer du seed-dataene i `Storage/Seed.cs`, må du samtidig endre `store/Menu.java` og `store/SeedOrders.java` i Java-fasiten og `4-Frontend/kaffebar-web/src/mocks/store.ts`.

5. **Skriv endepunktet.** I en controller er `GetOrder` malen:

   ```csharp
   /// <summary>Hent én bestilling.</summary>
   [HttpGet("{orderId:guid}")]
   [EndpointSummary("Hent én bestilling")]
   [ProducesResponseType<Order>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public ActionResult<Order> GetOrder(Guid orderId)
   {
       var order = repository.Find(orderId);
       return order is null ? NotFoundProblem(orderId) : Ok(order);
   }
   ```

   - En ny controller får `[ApiController]`, `[Route("…")]` og 400-svaret (`[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]`) på klassen. Den arver fra `ControllerBase` og tar avhengighetene i primærkonstruktøren, slik `OrdersController` gjør.
   - `[EndpointSummary]` på norsk blir `summary` i spesifikasjonen. Legg til `[EndpointDescription]` når oppførselen trenger en forklaring, for eksempel sortering eller default-verdier. `<remarks>` forklarer *hvorfor* endepunktet er laget slik.
   - Hver statuskode får sin egen `[ProducesResponseType]`, og feilsvar bruker `ProblemDetails` med `"application/problem+json"`.
   - Route-parametere får en constraint, som `{orderId:guid}`. En ugyldig guid gir da 404, mens Java-sporet gir 400. Forskjellen er bevisst (FASIT, oppgave 5).
   - Query-parametere skrives som vanlige parametere med `[FromQuery]`, en default-verdi og `[Range]`, slik `ListOrders` gjør. En record bundet med `[FromQuery]` mister default-verdiene.
   - Svar med `Ok(…)`, eller med `Created($"/orders/{id}", …)` for en ny ressurs. Alle feilsvar lages med `ProblemFactory` (`NotFound`, `Validation` eller `Create`) og `.AsResult()`, med norsk `detail`. Ikke sett `[Produces("application/json")]` på controlleren, for da får feilsvarene feil `Content-Type`.
   - Tilstandsendringer som er baristaens jobb, får `[Authorize(Policy = AuthPolicies.Barista)]`. Autentisering er av som standard, og da slipper policyen alle gjennom.
   - Legg felles logikk i private hjelpere, slik `ApplyStatus` og `NotFoundProblem` gjør.
   - Endrer endepunktet en ordre, publiserer du hendelsen med `OrderEventPublisher` (`OrderCreated` eller `OrderStatusChanged`), men bare når noe faktisk endret seg. Publiseringen skjer i bakgrunnen, så en broker som er nede gjør aldri et vellykket kall om til en 500.

   Et Minimal API legges i `Program.cs` etter `app.MapControllers()`, under en egen `// --- Minimal API: … ---`-overskrift:

   ```csharp
   app.MapGet("/menu", (IOrderRepository repository) => repository.Menu)
       .WithName("GetMenu")
       .WithSummary("Hent kaffemeny")
       .WithDescription("Returnerer en liste over alle tilgjengelige kaffedrikker i kaffebaren.")
       .Produces<IReadOnlyList<Coffee>>()
       .AllowAnonymous();
   ```

6. **Kontroller spesifikasjonen.** Start API-et med `dotnet run` i `Kaffebar/` (http://localhost:5035), og les `/openapi/v1.json` eller åpne Scalar på `/scalar/v1`. Sjekk at:
   - stien og metoden stemmer, og `summary` er på norsk
   - alle statuskodene er med, og feilsvarene har `application/problem+json`
   - enumene er strenger, `required`-lista stemmer, og valgfrie felt ikke tillater `null`

   Typer som følger konvensjonene i steg 3, dekkes av transformerne i `Kaffebar/OpenApi/`. Beskriver spesifikasjonen noe annet enn det API-et faktisk sender, retter du den med en transformer der, ikke ved å endre JSON-en. En ny JSON-converter må registreres begge steder i `Program.cs`: i `ConfigureJson` for controllerne og i `ConfigureHttpJsonOptions` for Minimal API-ene.

7. **Legg kallene i `Kaffebar/Kaffebar.http`.** Gi hvert kall et eget `###`-avsnitt med norsk overskrift, og bruk `{{host}}` og seed-id-ene (`c0ffee01-…` for menyen, `0de50001-…` for ordrene). Ta med både det vellykkede kallet og hvert feiltilfelle, for eksempel `### FEIL: ukjent ordre -> 404`. Bruk `# @name` når et senere kall trenger svaret.

8. **Skriv testene i `Kaffebar.Tests/KaffebarApiTests.cs`.**
   - Test over HTTP med `_client`. Ikke kall controller-metoden direkte, for statuskoder, headere, media types og serialisering hører til HTTP-laget.
   - Legg testene under en seksjonskommentar for endepunktet, i samme stil som de andre. Gi hver test `[Fact(DisplayName = "…")]` på norsk og et metodenavn på formen `Operasjon_Forventning`, for eksempel `GetOrder_ReturnsNotFound`.
   - Les svaret som `JsonElement`, og sjekk det som går på tråden: statuskode, feltnavn i camelCase, enum-strenger og `Location` ved 201.
   - Sjekk at feilsvar har `application/problem+json`, at `type` inneholder sluggen (`not-found`, `validation-error`), og at `errors[].field` peker på feltet ved 400.
   - Bruk konstantene (`ProblemJson`, `SeededPending`, `Latte`) og hjelperne `PostJson` og `PatchJson`. `%ID%` i JSON-en byttes ut med id-en til Kaffe Latte. Har du lagt til et nytt repository, må testkonstruktøren også kalle `Reset()` på det.

9. **Hold sporene og dokumentasjonen i takt.**
   - Hører endepunktet til den delte kontrakten, må Java-fasiten få det samme i `kaffebar-api.yaml`. Der endrer du YAML-en, kjører `mvn clean compile` og implementerer det genererte grensesnittet.
   - Skal frontenden bruke endepunktet, regenererer du typene i `4-Frontend/kaffebar-web` mot API-et mens det kjører: `KAFFEBAR_API_URL=http://localhost:5035 yarn generate:types`. I PowerShell skriver du `$env:KAFFEBAR_API_URL="http://localhost:5035"; yarn generate:types`.
   - Oppdater `FASIT.md` hvis endepunktet hører til en oppgave, og rett antallet tester begge stedene FASIT oppgir det: kodeblokken øverst og filtreet.

10. **Kjør testene** fra `3-APIs/kaffebar-dotnet-solution`. Stopp API-et først hvis det fortsatt kjører fra steg 6 eller 9. På Windows låser det filene i `bin/`, og da feiler bygget.

    ```powershell
    dotnet test
    ```

    Alle testene skal være grønne, også de som fantes fra før. Mens du jobber, kan du kjøre én test med `dotnet test --filter "FullyQualifiedName~GetOrder_ReturnsNotFound"`. `NU1903`-advarslene om sårbare pakker fantes fra før og kan ignoreres.
