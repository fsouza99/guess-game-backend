using App.Data;
using Microsoft.EntityFrameworkCore;

namespace App.StaticTools;

public static class DatabaseWorkloadsExtensions
{
    // Initialize the database by creating the models and adding placeholder data.
    public static async Task InitializeDatabaseAsync(
        this WebApplication app, bool useDbServer)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();

            await context.Database.EnsureCreatedAsync();
            if (await context.Formula.AnyAsync())
            {
                // Leave if data is created.
                return;
            }

            var dbinit = new DbInitializer(services, context);
            if (useDbServer)
            {
                /* "Formula" and "Competition" have "auto IDs" on SQL Server. With that,
                    no manual ID setting is allowed.

                    Although SQL "SET" commands could solve this, we judge it more
                    adequate to avoid insertion on these models directly from code.
                    Instead, an associated procedure must be made available for execution
                    on the database.

                    When using SQL Server, insert only user-related data from code.
                */
                await dbinit.AddUserDataToContext();
            }
            else
            {
                // If using SQLite, insert placeholder data for all models.
                await dbinit.AddUserAndBusinessDataToContext();
            }
            return;
        }
    }
}
