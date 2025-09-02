# Lilith Backend - Copilot Instructions

## Solution Overview

Lilith Backend is a comprehensive manufacturing ERP system built with .NET 8 using Clean Architecture principles. The solution manages the complete manufacturing lifecycle including purchases, sales, production planning, inventory, and financial operations for a manufacturing company.

## Architecture & Structure

### Project Organization
The solution follows Clean Architecture with 4 main projects:

1. **Domain** - Core business entities, interfaces, and domain logic
2. **Application** - Use cases, DTOs, and service interfaces 
3. **Infrastructure** - Data access, external services, and infrastructure concerns
4. **Api** - Web API controllers and application services

### Key Architectural Patterns

#### Clean Architecture Layers
- **Domain Layer**: Contains entities, value objects, domain services, and repository interfaces
- **Application Layer**: Contains application services, DTOs, and use case interfaces
- **Infrastructure Layer**: Implements repositories, data access, and external integrations
- **API Layer**: Contains controllers and application-specific services

#### Repository Pattern
- Generic `IRepository<TEntity, TId>` interface for common CRUD operations
- Specialized repositories for complex queries (e.g., `IWorkOrderRepository`, `ISalesOrderHeaderRepository`)
- Unit of Work pattern via `IUnitOfWork` to manage transactions and repository instances

#### Entity Framework & Data Access
- PostgreSQL database with Entity Framework Core
- Database views for reporting (prefixed with `vw_`)
- Soft delete pattern using `Disabled` property on base `Entity` class
- Decimal precision configured as (18,4) for amounts, (18,2) for prices

## Domain Model

### Base Entity
All entities inherit from abstract `Entity` class:
```csharp
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public bool Disabled { get; set; } = false;
}
```

### Core Business Areas

#### Production Management
- **WorkMaster**: Manufacturing routes/processes for references
- **WorkOrder**: Actual production orders with phases and details
- **WorkOrderPhase**: Individual manufacturing phases within work orders
- **WorkOrderPhaseDetail**: Detailed steps within phases
- **ProductionPart**: Time tracking and cost capture for production activities
- **Workcenter**: Manufacturing resources and machine centers

#### Sales Management
- **Customer**: Client management with addresses and contacts
- **SalesOrderHeader/Detail**: Sales orders with line items
- **DeliveryNote**: Shipping documents
- **SalesInvoice**: Customer invoicing with due dates and tax calculations
- **Budget**: Customer quotations

#### Purchase Management
- **Supplier**: Vendor management with references and pricing
- **PurchaseOrder/Detail**: Purchase orders with reception tracking
- **Receipt**: Goods receipt with weight/dimension calculations
- **PurchaseInvoice**: Vendor invoicing and payment tracking

#### Shared Components
- **Reference**: Product/service catalog with pricing and specifications
- **Lifecycle/Status**: Workflow state management across all business processes
- **Exercise**: Fiscal periods with document numbering sequences

## Common Patterns & Conventions

### Response Pattern
Use `GenericResponse` for service layer returns:
```csharp
public class GenericResponse
{
    public bool Result { get; set; }
    public object? Content { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

### Controller Conventions
- Use `[ApiController]` and `[Route("api/[controller]")]` attributes
- Implement standard CRUD operations: Create, GetById, GetAll, Update, Delete
- Use DTOs for complex requests (e.g., `CreateHeaderRequest`, `ChangeStatusRequest`)
- Validate `ModelState` before processing requests
- Return appropriate HTTP status codes (200, 201, 400, 404, 409)

### Service Layer Patterns
- Services inject `IUnitOfWork` for data access
- Use lifecycle/status management for workflow control
- Implement business validation before persistence
- Generate document numbers via `IExerciseService.GetNextCounter()`
- Handle related entity updates (e.g., updating stock, changing order statuses)

### Repository Implementations
- Generic `Repository<TEntity, TId>` for basic CRUD
- Specialized repositories inherit and extend functionality
- Use `Find()` for synchronous queries, `FindAsync()` for async
- Implement `UpdateWithoutSave()` for batch operations with manual `CompleteAsync()`

### Database Configuration
- Entity configurations in `Infrastructure/Persistance/EntityConfiguration/`
- Use `EntityBaseConfiguration.ConfigureBase<T>()` for common entity properties
- UUID primary keys with `ValueGeneratedNever()`
- Timestamp columns with PostgreSQL-specific types
- Decimal precision: (18,4) for general amounts, (18,2) for prices

## Business Logic Patterns

### Lifecycle Management
All major business entities use lifecycle/status management:
- Each entity type has a named lifecycle (e.g., "WorkOrder", "SalesOrder", "PurchaseInvoice")
- Status transitions control business workflow
- Use `ILifecycleRepository.GetStatusByName(lifecycleName, statusName)` to get specific statuses
- Initial status set from `Lifecycle.InitialStatusId`

### Document Numbering
- Documents use sequential numbering per exercise/fiscal period
- Generate via `IExerciseService.GetNextCounter(exerciseId, documentType)`
- Document types: "workorder", "salesorder", "purchaseorder", "salesinvoice", etc.

### Cost Calculation
- Work orders track operator, machine, material, and external costs
- Use `IMetricsService.GetWorkmasterMetrics()` for cost calculations
- Costs flow from work orders to references and sales order details
- Production parts capture actual time and costs vs. estimates

### Stock Management
- Stock movements track inventory changes
- Delivery notes consume stock when delivered
- Receipts add to stock when received
- Use `IStockService` for inventory operations

## Language & Localization

The system implements **comprehensive multilanguage support** with full internationalization across all business logic.

### Supported Languages
- **Catalan (ca)** - Default language
- **Spanish (es)** - Secondary language
- **English (en)** - International support

### Localization Architecture
- **JSON-based resource files** in `Api/Resources/LocalizationService/`
- **ILocalizationService** for dependency injection and string retrieval
- **CultureMiddleware** for automatic language detection
- **Microsoft.Extensions.Localization** framework integration
- **StatusConstants** in `Api/Constants/StatusConstants.cs` for database-stored values

### Culture Detection Priority
1. Query parameter: `?culture=ca`
2. Accept-Language header
3. Default to Catalan (ca)

### Database vs User-Facing Strings
- **Database Values**: Lifecycle and status names remain in Catalan in the database
- **Constants**: Use `StatusConstants` to reference database values (prevents typos)
- **User Messages**: All error messages and UI text support full localization

### Constants Usage
Use constants instead of magic strings for database-stored values:
```csharp
// Using Constants (Recommended)
var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == StatusConstants.Lifecycles.Budget).FirstOrDefault();
var status = await _unitOfWork.Lifecycles.GetStatusByName(StatusConstants.Lifecycles.WorkOrder, StatusConstants.Statuses.Creada);

// Available Constants:
StatusConstants.Lifecycles.Budget
StatusConstants.Lifecycles.SalesOrder
StatusConstants.Lifecycles.WorkOrder
StatusConstants.Statuses.Creada
StatusConstants.Statuses.PendentAcceptar
StatusConstants.Sites.LocalTorello
```

### Using Localization in Services
All API services must inject and use `ILocalizationService`:
```csharp
public class ExampleService : IExampleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    
    public ExampleService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
    }
    
    public async Task<GenericResponse> SomeMethod(Guid id)
    {
        var entity = await _unitOfWork.Entities.Get(id);
        if (entity == null)
        {
            return new GenericResponse(false, _localizationService.GetLocalizedString("EntityNotFound", id));
        }
        
        // Business logic here
        return new GenericResponse(true, entity);
    }
}
```

### Using Localization in Controllers
```csharp
public class ExampleController : ControllerBase
{
    private readonly ILocalizationService _localizationService;
    
    public ExampleController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IActionResult SomeAction(Guid id)
    {
        // For current request culture
        var message = _localizationService.GetLocalizedString("EntityNotFound", id);
        
        // For specific culture
        var spanishMessage = _localizationService.GetLocalizedStringForCulture("EntityNotFound", "es", id);
        
        return NotFound(new GenericResponse(false, message));
    }
}
```

### Localization Methods
- **GetLocalizedString(key, params args)** - Uses current request culture
- **GetLocalizedStringForCulture(key, culture, params args)** - Forces specific culture

### Standard Localization Keys
All services use standardized localization keys organized by category:

#### Entity Operations
- `EntityNotFound`: "Entity with ID {0} not found"
- `EntityAlreadyExists`: "Entity already exists"
- `EntityDisabled`: "Entity with ID {0} is disabled"
- `Common.IdNotExist`: "Id {0} does not exist"

#### Business Entities
- `CustomerNotFound`: "Customer not found"
- `CustomerInvalid`: "Customer is not valid for creating an invoice..."
- `WorkOrderNotFound`: "Work order with ID {0} not found"
- `BudgetNotFound`: "Budget with ID {0} not found"
- `InvoiceNotFound`: "Invoice with ID {0} not found"

#### Exercise & Document Management
- `ExerciseNotFound`: "Exercise not found"
- `ExerciseCounterError`: "Error creating counter"
- `ExerciseCounterNotFound`: "The provided counter '{0}' is not valid"

#### Lifecycle & Status Management
- `LifecycleNotFound`: "Lifecycle '{0}' not found"
- `LifecycleNoInitialStatus`: "Lifecycle '{0}' has no initial status"
- `StatusNotFound`: "Status with ID {0} not found or is disabled"

#### Validation Messages
- `Validation.Required`: "The {0} field is required"
- `Validation.InvalidEmail`: "Email format is not valid"

#### Authentication
- `UserNotFound`: "User not found"
- `UserPasswordInvalid`: "Password is not valid"
- `AuthTokenExpired`: "Token expired"

### Language File Structure
Each language file (ca.json, es.json, en.json) contains categorized keys:

```json
{
  "EntityNotFound": "Entity with ID {0} not found",
  "CustomerNotFound": "Customer not found",
  "CustomerInvalid": "Customer is not valid for creating an invoice...",
  "ExerciseCounterError": "Error creating counter",
  "LifecycleNotFound": "Lifecycle '{0}' not found",
  "StatusNames.Created": "Created",
  "Validation.Required": "The {0} field is required",
  "Movement.AlbaranDescription": "Delivery note {0}",
  "Common.IdNotExist": "Id {0} does not exist"
}
```

## API Conventions

### Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
public class EntityController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntityService _entityService; // if complex business logic
    private readonly ILocalizationService _localizationService; // REQUIRED for multilanguage support
    
    // Standard CRUD + specialized endpoints
    // Sub-resource management (e.g., /Entity/{id}/SubEntity)
}
```

### Standard Responses
- `200 OK`: Successful operations, return entity or collection
- `201 Created`: Entity creation, return entity and Location header
- `400 BadRequest`: Validation errors, return `ModelState.ValidationState`
- `404 NotFound`: Entity not found, return localized `GenericResponse` with error message
- `409 Conflict`: Business rule violations, return localized `GenericResponse` with error message

### Sub-Resource Management
Many entities have sub-resources (e.g., WorkMaster has Phases, Phases have Details):
- Separate endpoints for sub-resource CRUD
- Use Swagger annotations for API documentation
- Validate parent entity existence before sub-resource operations

## Development Guidelines

### When Adding New Features
1. **Domain First**: Define entities in Domain layer with proper relationships
2. **Repository Pattern**: Create specialized repositories if complex queries needed
3. **Service Layer**: Implement business logic in Application services
4. **Controller Layer**: Create RESTful endpoints following existing conventions
5. **Validation**: Use data annotations and business validation
6. **Localization**: Add new message keys to all language JSON files
7. **Testing**: Consider integration tests for complex workflows

### Error Handling
- Use `GenericResponse` for business logic errors
- Return appropriate HTTP status codes
- Validate `ModelState` in controllers
- Handle entity not found scenarios consistently
- **ALWAYS use localized error messages** via `ILocalizationService`
- **NEVER use hardcoded strings** in business logic

### Localization Implementation Requirements

#### For All New Services:
1. **Inject ILocalizationService** in constructor
2. **Use StatusConstants** for database-stored values
3. **Add localization keys** to all 3 language files (ca.json, es.json, en.json)
4. **Use parameterized messages** for dynamic content
5. **Follow established key naming patterns**

#### Adding New Localization Keys:
```csharp
// 1. Add to ca.json
"MyNewErrorKey": "El meu nou missatge d'error amb {0}"

// 2. Add to es.json  
"MyNewErrorKey": "Mi nuevo mensaje de error con {0}"

// 3. Add to en.json
"MyNewErrorKey": "My new error message with {0}"

// 4. Use in service
return new GenericResponse(false, _localizationService.GetLocalizedString("MyNewErrorKey", someValue));
```

#### Constants Pattern:
```csharp
// Add to Api/Constants/StatusConstants.cs
public static class StatusConstants
{
    public static class Lifecycles
    {
        public const string MyNewLifecycle = "MyNewLifecycle";
    }
    
    public static class Statuses  
    {
        public const string MyNewStatus = "MyNewStatus"; // In Catalan as stored in DB
    }
}
```

### Localization Best Practices
- **Add new strings to all language files** (ca.json, es.json, en.json)
- **Use parameterized messages** for dynamic content: `_localizationService.GetLocalizedString("EntityNotFound", id)`
- **Use GetLocalizedStringForCulture** when you need to force a specific culture
- **Test with different cultures** using query parameter: `?culture=en`
- **Maintain consistent key naming** following established patterns
- **Prefer specific keys** over generic ones for better translations
- **Use constants for database values** to prevent typos and ensure consistency
- **Group related keys** with dot notation (e.g., `Validation.Required`, `StatusNames.Created`)

### Performance Considerations
- Use async/await for all database operations
- Prefer `Find()` over `GetAll().Where()` for filtering
- Use specialized repositories for complex queries
- Clear navigation properties to avoid EF tracking issues

### Code Style
- Follow C# naming conventions
- Use nullable reference types appropriately  
- Prefer explicit types over `var` for clarity
- Comment business logic in English for international team
- Use meaningful variable names reflecting business concepts

## Localization Implementation Status

All 15 API services have been fully updated with comprehensive localization support:

### ? Fully Localized Services:
- **Sales**: BudgetService, SalesOrderService, SalesInvoiceService, DeliveryNoteService
- **Purchase**: PurchaseOrderService, PurchaseInvoiceService, ReceiptService
- **Production**: WorkOrderService, MetricsService
- **Warehouse**: StockMovementService
- **Shared**: ExerciseService, ReferenceService, FileService, AuthenticationService

### Key Achievements:
- **40+ localization keys** covering all business scenarios
- **Constants for database values** preventing runtime errors
- **Consistent error handling** patterns across all services
- **Full multilingual support** for Catalan, Spanish, and English
- **Parameterized messages** for dynamic content
- **Type-safe constants** for lifecycle and status references

This solution represents a comprehensive manufacturing ERP with sophisticated business workflows, multi-entity relationships, financial tracking capabilities, and **complete multilanguage support**. When working with this codebase, prioritize understanding the business domain, maintaining consistency with established patterns, and ensuring all user-facing messages are properly localized using the established localization infrastructure.