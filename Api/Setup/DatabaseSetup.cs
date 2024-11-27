﻿using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Setup;

public static class DatabaseSetup
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(Configuration.ConnectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services
            .AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
