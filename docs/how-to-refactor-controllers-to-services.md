# How to Refactor Controllers to Services

This guide explains how to extract business logic from controllers into dedicated service classes, following Clean Architecture principles and established patterns.

## When to Apply This Refactoring

✅ **Apply when:**

- Controller has business logic mixed with HTTP concerns
- Controller directly injects `IUnitOfWork` and performs data access
- Controller contains validation, entity existence checks, or complex orchestration
- Multiple controllers share similar business logic patterns

**Reference implementation:** `LifecycleController` refactoring (completed December 2025)

---

## Architecture Overview

```
Controller (Api) → Service (Application) → Repository (Infrastructure) → Database
     HTTP              Business Logic          Data Access
```

**Layer responsibilities:**

- **Api** - HTTP concerns only (routing, status codes, request/response)
- **Application** - Business logic, validation, workflow orchestration
- **Application.Contracts** - Service and repository interfaces, DTOs
- **Infrastructure** - Repository implementations, EF Core data access

---

## Step 1: Create Service Interface (Application.Contracts)

**Location:** `src/Application.Contracts/Services/I{EntityName}Service.cs`

**Pattern:**

```csharp
namespace Application.Contracts;

public interface I{EntityName}Service
{
    // Read operations: Return entities directly (nullable for single, enumerable for lists)
    Task<{Entity}?> GetById(Guid id);
    Task<{Entity}?> GetByName(string name);
    Task<IEnumerable<{Entity}>> GetAll();

    // Write operations: Return GenericResponse for error handling
    Task<GenericResponse> Create({Entity} entity);
    Task<GenericResponse> Update(Guid id, {Entity} entity);
    Task<GenericResponse> Remove(Guid id);

    // Complex operations: Use request DTOs if needed
    Task<GenericResponse> ChangeStatus(Guid id, Guid newStatusId);
    Task<GenericResponse> ProcessComplexOperation(ComplexRequest request);
}
```

**Key Points:**

- ✅ Simple reads return `Task<Entity?>` or `Task<IEnumerable<Entity>>`
- ✅ Writes return `Task<GenericResponse>`
- ✅ Complex operations can return `GenericResponse` with Content
- ✅ Use request DTOs for operations with multiple parameters

---

### Step 2: Implement Service (Api Layer)

**Location:** `Api/Services/{Domain}/{EntityName}Service.cs`

**Pattern:**

```csharp
using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Api.Services.{Domain};

public class {EntityName}Service(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IExerciseService exerciseService  // If document numbering needed
) : I{EntityName}Service
{
    // Read operations - return entities directly
    public async Task<{Entity}?> Get{Entity}ById(Guid id)
    {
        return await unitOfWork.{Entity}s.Get(id);
    }

    public async Task<IEnumerable<{Entity}>> GetAll{Entity}s()
    {
        var entities = await unitOfWork.{Entity}s.GetAll();
        return entities.OrderBy(e => e.Name); // Optional sorting
    }

    // Write operations - return GenericResponse
    public async Task<GenericResponse> Create{Entity}({Entity} entity)
    {
        // 1. Validation
        var exists = unitOfWork.{Entity}s.Find(e => e.Name == entity.Name).Any();
        if (exists)
        {
            var message = localizationService.GetLocalizedString("EntityAlreadyExists");
            return new GenericResponse(false, message);
        }

        // 2. Document numbering (if applicable)
        if (entity.RequiresNumbering)
        {
            var counterObj = await exerciseService.GetNextCounter(entity.ExerciseId, "{entity}");
            if (!counterObj.Result || counterObj.Content == null)
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("ExerciseCounterError"));
            entity.Number = counterObj.Content.ToString()!;
        }

        // 3. Set initial status from lifecycle (if applicable)
        var lifecycle = unitOfWork.Lifecycles.Find(l =>
            l.Name == StatusConstants.Lifecycles.{Entity}).FirstOrDefault();
        if (lifecycle == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("LifecycleNotFound",
                    StatusConstants.Lifecycles.{Entity}));
        entity.StatusId = lifecycle.InitialStatusId;

        // 4. Persist
        await unitOfWork.{Entity}s.Add(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<GenericResponse> Update{Entity}({Entity} entity)
    {
        // 1. Clear navigation properties to avoid EF tracking issues
        entity.RelatedEntities?.Clear();

        // 2. Validate existence
        var existing = await unitOfWork.{Entity}s.Get(entity.Id);
        if (existing == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("{Entity}NotFound", entity.Id));
        }

        // 3. Business validation
        // Add any specific business rules here

        // 4. Update
        await unitOfWork.{Entity}s.Update(entity);
        return new GenericResponse(true, entity);
    }

    public async Task<GenericResponse> Remove{Entity}(Guid id)
    {
        var entity = unitOfWork.{Entity}s.Find(e => e.Id == id).FirstOrDefault();
        if (entity == null)
        {
            return new GenericResponse(false,
                localizationService.GetLocalizedString("{Entity}NotFound", id));
        }

        await unitOfWork.{Entity}s.Remove(entity);
        return new GenericResponse(true, entity);
    }
}
```

**Key Points:**

- ✅ Use **primary constructor pattern** (C# 12)
- ✅ ALL error messages use `localizationService.GetLocalizedString()`
- ✅ NEVER use hardcoded error strings
- ✅ Use `StatusConstants` for lifecycle/status names (not magic strings)
- ✅ Clear navigation properties before updates to avoid EF tracking issues
- ✅ Return entity in `GenericResponse.Content` for Create operations
- ✅ Inject other services as needed (`IExerciseService`, domain services)

---

### Step 3: Register Service in DI Container

**Location:** `Api/Setup/ApplicationServicesSetup.cs`

**Add namespace import:**

```csharp
using Api.Services.{Domain};
```

**Add registration:**

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // ... existing registrations ...

    services.AddScoped<I{EntityName}Service, {EntityName}Service>();

    // ... more registrations ...
    return services;
}
```

**Key Points:**

- ✅ Use `AddScoped` for services that access database
- ✅ Use `AddSingleton` only for stateless utility services (e.g., `IQrCodeService`)
- ✅ Keep registrations alphabetically organized by domain

---

### Step 4: Refactor Controller

**Location:** `Api/Controllers/{Domain}/{EntityName}Controller.cs`

**Before:**

```csharp
public class {EntityName}Controller(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService
) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await unitOfWork.{Entity}s.Get(id);
        if (entity is not null)
        {
            return Ok(entity);
        }
        else
        {
            var message = localizationService.GetLocalizedString("EntityNotFound", id);
            return NotFound(new GenericResponse(false, message));
        }
    }
}
```

**After:**

```csharp
public class {EntityName}Controller(I{EntityName}Service service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await service.Get{Entity}ById(id);
        if (entity is not null)
            return Ok(entity);
        else
            return NotFound();
    }
}
```

**Complete Controller Pattern:**

```csharp
using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.{Domain}
{
    [ApiController]
    [Route("api/[controller]")]
    public class {EntityName}Controller(I{EntityName}Service service) : ControllerBase
    {
        // GET all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await service.GetAll{Entity}s();
            return Ok(entities);
        }

        // GET by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await service.Get{Entity}ById(id);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        // POST create
        [HttpPost]
        public async Task<IActionResult> Create({Entity} request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Create{Entity}(request);
            if (response.Result)
            {
                var location = Url.Action(nameof(GetById), new { id = request.Id })
                    ?? $"/{request.Id}";
                return Created(location, response.Content);
            }
            else
            {
                return Conflict(response);
            }
        }

        // PUT update
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, {Entity} request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (id != request.Id)
                return BadRequest();

            var response = await service.Update{Entity}(request);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }

        // DELETE
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var response = await service.Remove{Entity}(id);
            if (response.Result)
                return Ok(response.Content);
            else
                return NotFound(response);
        }
    }
}
```

**HTTP Status Code Mapping:**

- ✅ `200 OK` - Successful GET/PUT/DELETE with content
- ✅ `201 Created` - Successful POST with Location header
- ✅ `400 BadRequest` - Validation errors, malformed requests
- ✅ `404 NotFound` - Entity not found (reads) or business validation failed (writes)
- ✅ `409 Conflict` - Entity already exists or business rule violation

**Key Points:**

- ✅ Controller only handles HTTP concerns (routing, status codes)
- ✅ Remove `IUnitOfWork` and `ILocalizationService` from controller
- ✅ Inject only the service interface
- ✅ All business logic delegated to service
- ✅ Map `GenericResponse.Result` to HTTP status codes
- ✅ Return entity for successful reads (no wrapper)
- ✅ Return `response.Content` for successful writes

---

## Common Patterns & Best Practices

### Localization Keys Pattern

**Standard Keys (Already Available):**

```csharp
"EntityNotFound"              // Generic entity not found with {0} = ID
"EntityAlreadyExists"         // Generic entity already exists
"EntityDisabled"              // Entity is disabled with {0} = ID
"{Entity}NotFound"            // Specific entity not found (e.g., "BudgetNotFound")
"LifecycleNotFound"           // Lifecycle '{0}' not found
"StatusNotFound"              // Status with ID {0} not found
"ExerciseCounterError"        // Error creating document counter
"Validation.Required"         // Field {0} is required
```

**Adding New Keys:**

1. Add to `Api/Resources/LocalizationService/ca.json`
2. Add to `Api/Resources/LocalizationService/es.json`
3. Add to `Api/Resources/LocalizationService/en.json`

**Example:**

```json
// ca.json
"CustomEntityNotFound": "L'entitat personalitzada amb ID {0} no existeix"

// es.json
"CustomEntityNotFound": "La entidad personalizada con ID {0} no existe"

// en.json
"CustomEntityNotFound": "Custom entity with ID {0} not found"
```

---

### StatusConstants Pattern

**Usage:** Access database-stored lifecycle/status names via constants to prevent typos.

**Location:** `Api/Constants/StatusConstants.cs`

**Example:**

```csharp
public static class StatusConstants
{
    public static class Lifecycles
    {
        public const string Budget = "Budget";
        public const string SalesOrder = "SalesOrder";
        public const string WorkOrder = "WorkOrder";
        public const string MyNewEntity = "MyNewEntity";  // Add new
    }

    public static class Statuses
    {
        public const string Creada = "Creada";  // In Catalan (as stored in DB)
        public const string PendentAcceptar = "Pendent d'acceptar";
        public const string Acceptat = "Acceptat";
    }
}
```

**Service Usage:**

```csharp
// ❌ WRONG - Magic string
var lifecycle = unitOfWork.Lifecycles.Find(l => l.Name == "Budget").FirstOrDefault();

// ✅ CORRECT - Using constant
var lifecycle = unitOfWork.Lifecycles.Find(l =>
    l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
```

---

### Document Numbering Pattern

**For entities requiring sequential document numbers:**

```csharp
public async Task<GenericResponse> CreateWithNumber(CreateRequest request)
{
    // Generate document number from exercise
    var counterObj = await exerciseService.GetNextCounter(
        request.ExerciseId,
        "documentType"  // e.g., "budget", "salesorder", "workorder"
    );

    if (!counterObj.Result || counterObj.Content == null)
    {
        return new GenericResponse(false,
            localizationService.GetLocalizedString("ExerciseCounterError"));
    }

    var entity = new Entity
    {
        Id = request.Id,
        Number = counterObj.Content.ToString()!,
        // ... other properties
    };

    await unitOfWork.Entities.Add(entity);
    return new GenericResponse(true, entity);
}
```

**Document Types:**

- `"budget"` - Customer quotations
- `"salesorder"` - Sales orders
- `"workorder"` - Manufacturing orders
- `"salesinvoice"` - Customer invoices
- `"purchaseorder"` - Purchase orders
- `"purchaseinvoice"` - Vendor invoices

---

### Nested Entity Management Pattern

**For entities with sub-resources (e.g., Order → Details, WorkOrder → Phases):**

```csharp
// Parent operations
public async Task<GenericResponse> CreateOrder(Order order)
{
    await unitOfWork.Orders.Add(order);
    return new GenericResponse(true, order);
}

// Child operations - re-fetch parent after changes
public async Task<GenericResponse> AddDetail(OrderDetail detail)
{
    await unitOfWork.OrderDetails.Add(detail);

    // Re-fetch parent to ensure UI has updated data
    var parent = await unitOfWork.Orders.Get(detail.OrderId);
    return new GenericResponse(true, parent);
}

public async Task<GenericResponse> UpdateDetail(Guid id, OrderDetail detail)
{
    var existing = await unitOfWork.OrderDetails.Get(id);
    if (existing == null)
        return new GenericResponse(false,
            localizationService.GetLocalizedString("EntityNotFound", id));

    await unitOfWork.OrderDetails.Update(detail);

    // Re-fetch parent
    var parent = await unitOfWork.Orders.Get(detail.OrderId);
    return new GenericResponse(true, parent);
}
```

**Controller Pattern for Sub-Resources:**

```csharp
// POST /api/order/{orderId}/detail
[Route("{orderId:guid}/detail")]
[HttpPost]
public async Task<IActionResult> AddDetail(Guid orderId, OrderDetail request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState.ValidationState);

    request.OrderId = orderId; // Ensure parent ID is set

    var response = await service.AddDetail(request);
    if (response.Result)
        return Ok(response.Content); // Returns updated parent
    else
        return BadRequest(response);
}
```

---

## Migration Checklist

Use this checklist when refactoring a controller:

### ☐ Step 1: Create Service Interface

- [ ] Created `Application/Services/I{Name}Service.cs`
- [ ] Defined all CRUD method signatures
- [ ] Read operations return entities directly (`Task<Entity?>`)
- [ ] Write operations return `Task<GenericResponse>`
- [ ] Added required using statements (`Application.Contracts`, `Domain.Entities`)

### ☐ Step 2: Implement Service

- [ ] Created `Api/Services/{Domain}/{Name}Service.cs`
- [ ] Used primary constructor pattern with dependencies
- [ ] Injected `IUnitOfWork` and `ILocalizationService`
- [ ] Injected additional services if needed (`IExerciseService`, etc.)
- [ ] ALL error messages use `localizationService.GetLocalizedString()`
- [ ] Used `StatusConstants` for lifecycle/status references (no magic strings)
- [ ] Cleared navigation properties before updates
- [ ] Implemented all interface methods with proper error handling
- [ ] Returned entities in `GenericResponse.Content` where appropriate

### ☐ Step 3: Register Service

- [ ] Added service registration in `ApplicationServicesSetup.cs`
- [ ] Added namespace import for service implementation
- [ ] Used `AddScoped<IService, Service>()`

### ☐ Step 4: Refactor Controller

- [ ] Changed constructor to inject only `IService` (removed IUnitOfWork, ILocalizationService)
- [ ] Updated all endpoints to delegate to service
- [ ] Mapped `GenericResponse.Result` to HTTP status codes
- [ ] Simplified read operations (return entity or NotFound)
- [ ] Removed business logic from controller
- [ ] Kept only HTTP concerns (routing, status codes, ModelState validation)

### ☐ Step 5: Verify & Test

- [ ] Added necessary `using` statements (check Domain.Entities if using Status/StatusTransition)
- [ ] Verified all localization keys exist in ca.json, es.json, en.json
- [ ] Ran `dotnet build Api/Api.csproj` - Build successful
- [ ] No compilation errors
- [ ] Manually tested endpoints (optional)
- [ ] Verified error messages are localized

---

## Benefits of This Pattern

### Separation of Concerns

- **Controller**: HTTP routing, status code mapping, request/response shaping
- **Service**: Business logic, validation, orchestration, error handling
- **Repository**: Data access, queries, persistence

### Testability

- Service layer can be unit tested without HTTP context
- Controllers can be tested by mocking service interface
- Business logic isolated from infrastructure concerns

### Maintainability

- Business logic centralized and reusable
- Consistent error handling via GenericResponse
- Localized error messages (multilingual support)
- Follows established patterns across codebase

### Type Safety

- Read operations return strongly-typed entities or null
- Write operations return GenericResponse with optional Content
- Compile-time verification of dependencies

### Reusability

- Services can be injected into other services
- Business logic shared across multiple controllers
- Report services can use domain services

---

## Example: Complete LifecycleController Migration

See the actual implementation (completed December 2025):

- **Interface**: `Application/Services/ILifecycleService.cs`
- **Implementation**: `Api/Services/Shared/LifecycleService.cs`
- **Controller**: `Api/Controllers/Shared/LifecycleController.cs`
- **Registration**: `Api/Setup/ApplicationServicesSetup.cs`

**Metrics:**

- Controller LOC: 167 (before) → 167 (after) - same size, but focused
- Controller responsibilities: 4 → 1 (HTTP only)
- Dependencies injected: 2 → 1
- Service LOC: 0 → 154 (new, testable business logic)
- Build time: 78.6s (successful)

---

## Troubleshooting

### Compilation Errors

**Error:** `The type or namespace name 'Status' could not be found`
**Solution:** Add `using Domain.Entities;` to service and controller files

**Error:** `Build failed with X error(s)`
**Solution:** Check all three layers have correct using statements:

- Application: `using Domain.Entities;`
- Api Service: `using Domain.Entities;`
- Api Controller: `using Domain.Entities;`

### Missing Localization Keys

**Error:** Key not found at runtime
**Solution:** Add missing keys to all 3 language files (ca.json, es.json, en.json)

### Service Not Resolved

**Error:** `Unable to resolve service for type 'IMyService'`
**Solution:** Verify service is registered in `ApplicationServicesSetup.cs`

---

## Advanced Patterns

### Service Composition

Services can inject other services for complex operations:

```csharp
public class SalesOrderService(
    IUnitOfWork unitOfWork,
    ILocalizationService localizationService,
    IExerciseService exerciseService,
    IBudgetService budgetService,  // Inject related domain service
    IStockService stockService
) : ISalesOrderService
{
    public async Task<GenericResponse> ConvertFromBudget(Guid budgetId)
    {
        // Use budgetService to get and validate budget
        var budget = await budgetService.GetBudgetById(budgetId);
        if (budget == null)
            return new GenericResponse(false,
                localizationService.GetLocalizedString("BudgetNotFound", budgetId));

        // Create sales order from budget
        var order = MapBudgetToOrder(budget);

        // Use exerciseService for document numbering
        var counterObj = await exerciseService.GetNextCounter(order.ExerciseId, "salesorder");
        // ... rest of logic
    }
}
```

### Background Services

For long-running operations (e.g., auto-reject outdated budgets):

```csharp
public class AutoRejectBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AutoRejectBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBudgetService>();

            await service.RejectOutdatedBudgets();

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken);
        }
    }
}

// Registration
services.AddHostedService<AutoRejectBackgroundService>();
```

---

## Summary

This refactoring pattern provides a **clean, testable, and maintainable architecture** that:

- Separates HTTP concerns from business logic
- Enables comprehensive testing without infrastructure dependencies
- Provides consistent error handling and localization
- Follows established patterns across the entire codebase
- Scales well as the application grows

**Apply this pattern systematically** to all controllers that mix business logic with HTTP concerns to achieve a true Clean Architecture implementation.
