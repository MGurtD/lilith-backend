# How to add a new read-only endpoint that queries the database

This guide explains how to add a new GET endpoint that queries the database and returns data using the Lilith backend architecture and conventions.

Architecture layers involved

- Domain: repository interfaces (contracts).
- Infrastructure: repository implementations (EF Core).
- Application: service interfaces and service logic.
- API: controllers and request/response shaping.

Conventions used here come from existing code like `ISalesInvoiceRepository`, `VerifactuIntegrationService`, and `VerifactuController`.

## 1) Add a repository method (Domain)

Define an explicit query contract on the corresponding repository interface under `Domain/Repositories/...`.

Example (Sales):

```csharp
// File: Domain/Repositories/Sales/ISalesInvoiceRepository.cs
public interface ISalesInvoiceRepository : IRepository<SalesInvoice, Guid>
{
		// ...existing members...
		Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate);
}
```

Notes

- Prefer Task-returning async methods for IO.
- Name methods after the query intent (e.g., BetweenDates, PendingToIntegrate).

## 2) Implement the repository (Infrastructure)

Implement the interface in `Infrastructure/Persistance/Repositories/...`. Use `AsNoTracking()` for read-only queries and include only what you need.

Example implementation:

```csharp
// File: Infrastructure/Persistance/Repositories/Sales/SalesInvoiceRepository.cs
public class SalesInvoiceRepository(ApplicationDbContext context)
		: Repository<SalesInvoice, Guid>(context), ISalesInvoiceRepository
{
		// ...existing code...

		public async Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate)
		{
				return await dbSet
						.AsNoTracking()
						.Include(i => i.VerifactuRequests) // include requests if needed by consumers
						.Where(i => i.VerifactuRequests.Any(r => r.CreatedOn >= fromDate && r.CreatedOn <= toDate))
						.OrderBy(i => i.InvoiceNumber)
						.ToListAsync();
		}
}
```

Tips

- Use columns with indexes in your filter for performance.
- Keep navigation Includes minimal. Return only what the service/controller needs.

## 3) Expose the method through a service (Application)

Add a method on the appropriate service interface under `Application/Services/...` and implement it. If the feature is integration-related, the implementation pattern in this codebase places it under `Api/Services/<Feature>/` (e.g., `Verifactu`).

Interface:

```csharp
// File: Application/Services/Verifactu/IVerifactuIntegrationService.cs
public interface IVerifactuIntegrationService
{
		// ...existing members...
		Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate);
}
```

Implementation:

```csharp
// File: Api/Services/Verifactu/VerifactuIntegrationService.cs
public class VerifactuIntegrationService : IVerifactuIntegrationService
{
		private readonly IUnitOfWork unitOfWork;
		// ...ctor and other members...

		public async Task<IEnumerable<SalesInvoice>> GetIntegrationsBetweenDates(DateTime fromDate, DateTime toDate)
		{
				if (toDate < fromDate) return Enumerable.Empty<SalesInvoice>();
				return await unitOfWork.SalesInvoices.GetIntegrationsBetweenDates(fromDate, toDate);
		}
}
```

Notes

- Keep business rules here (e.g., date validation). Heavy query composition belongs in the repository.

## 4) Add the controller endpoint (API)

Expose a GET endpoint with query parameters. Validate input and use localization for error messages.

```csharp
// File: Api/Controllers/Verifactu/VerifactuController.cs
[HttpGet("IntegrationsBetweenDates")]
[SwaggerOperation("GetInvoicesAndRequestsBetweenDates")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> GetIntegrationsBetweenDates([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
{
		if (toDate < fromDate)
				return BadRequest(localizationService.GetLocalizedString("InvalidDateRange"));

		var invoices = await service.GetIntegrationsBetweenDates(fromDate, toDate);
		return Ok(invoices);
}
```

Tips

- Use meaningful route names and `SwaggerOperation` for documentation.
- Bind query parameters explicitly with `[FromQuery]` when helpful.

## 5) Unit of Work and DI

If you add a brand-new repository:

- Add it to `IUnitOfWork` (Application/Persistance/IUnitOfWork.cs).
- Register the implementation in the Infrastructure DI configuration.
- Ensure the concrete UnitOfWork exposes it.

For existing repositories like `SalesInvoices`, no extra DI work is needed.

## 6) Validation and edge cases

- Date range: ensure `toDate >= fromDate`. Consider inclusive end date semantics.
- Time zones: decide whether the API expects UTC or local. ISO-8601 with timezone is recommended.
- Large result sets: consider paging parameters (`page`, `pageSize`) and server-side ordering.
- Security: ensure the endpoint requires appropriate auth/roles if needed.

## 7) Testing the endpoint quickly

PowerShell (encodes ISO-8601 UTC):

```powershell
$from = [DateTime]::UtcNow.AddDays(-7).ToString("o")
$to = [DateTime]::UtcNow.ToString("o")
Invoke-RestMethod -Uri "https://localhost:5001/api/Verifactu/IntegrationsBetweenDates?fromDate=$([uri]::EscapeDataString($from))&toDate=$([uri]::EscapeDataString($to))" -Method GET
```

cURL:

```bash
curl -G "https://localhost:5001/api/Verifactu/IntegrationsBetweenDates" \
	--data-urlencode "fromDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
	--data-urlencode "toDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

## 8) Optional: Frontend wiring (Vue + Pinia)

Service method example:

```ts
// src/modules/verifactu/services/verifactu.service.ts
async GetIntegrationsBetweenDates(fromDate: string | Date, toDate: string | Date): Promise<any[] | undefined> {
	const fromIso = new Date(fromDate).toISOString();
	const toIso = new Date(toDate).toISOString();
	const endpoint = `${this.resource}/IntegrationsBetweenDates?fromDate=${encodeURIComponent(fromIso)}&toDate=${encodeURIComponent(toIso)}`;
	const response = await this.apiClient.get(endpoint);
	if (response.status === 200) return response.data as any[];
}
```

Store action example:

```ts
// src/modules/verifactu/store/verifactu.ts
async GetIntegrationsBetweenDates(fromDate: string | Date, toDate: string | Date): Promise<any[] | undefined> {
	this.loading = true;
	try {
		const res = await VerifactuService.Verifactu.GetIntegrationsBetweenDates(fromDate, toDate);
		if (res) this.integrationsBetweenDates = res;
		return res;
	} finally {
		this.loading = false;
	}
}
```

## 9) Quality checks

- Build compiles: run the backend build task.
- Lint: ensure no style/type issues.
- Smoke test: hit the endpoint locally and verify JSON shape.

## 10) Quick checklist

- [ ] Repository interface updated
- [ ] Repository implementation added
- [ ] Service interface exposes method
- [ ] Service implementation calls repository
- [ ] Controller endpoint added with Swagger and validation
- [ ] DI/UnitOfWork updated (only if new repo)
- [ ] Manual/API test performed

```

```
