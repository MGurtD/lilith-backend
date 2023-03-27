using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    public static class CustomerController
    {
        public static void Configure(WebApplication app) {
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

            app.MapPut("/api/customer/{id:guid}", async (Guid id, Customer customer, ApplicationDbContext dbContext) =>
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
        }
    }
}
