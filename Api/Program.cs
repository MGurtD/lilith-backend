using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration
                           .AddUserSecrets("Lilith.Backend")
                           .AddEnvironmentVariables();

var connectionString = builder.Configuration["ConnectionString"] ?? "";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Api")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/customer", async (Customer customer, ApplicationDbContext dbContext) =>
{
    dbContext.Customers.Add(customer);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/customer/{customer.Id}", customer);
});

app.MapGet("api/customer/{id:guid}", async (Guid id, ApplicationDbContext dbContext) =>
{
    return await dbContext.Customers.FindAsync(id)
        is Customer customer 
        ? Results.Ok(customer) 
        : Results.NotFound();
});

app.MapGet("api/customer", async (ApplicationDbContext dbContext) =>
{
    var customers = await dbContext.Customers.ToListAsync();
    return Results.Ok(customers);
});

app.MapPut("/api/customer/{id:guid}", async(Guid id, Customer customer, ApplicationDbContext dbContext) =>
{
    if (customer.Id != id)
        return Results.BadRequest();

    var customerDb = await dbContext.Customers.FindAsync(id);
    if (customerDb == null)
        return Results.NotFound();

    customerDb = customer;
    await dbContext.SaveChangesAsync();

    return Results.Ok(customer);
});

app.MapDelete("/api/customer/{id:guid}", async (Guid id, ApplicationDbContext dbContext) =>
{
    var customerDb = await dbContext.Customers.FindAsync(id);
    if (customerDb is not null)
    {
        dbContext.Customers.Remove(customerDb);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }
    else
    {
        return Results.NotFound();
    }
        
});

// Aplicar automàticament les migracions pendents de EFCore
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();