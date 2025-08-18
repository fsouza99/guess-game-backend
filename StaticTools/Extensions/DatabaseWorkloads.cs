using App.Data;
using Microsoft.EntityFrameworkCore;

namespace App.StaticTools.Extensions;

public static class DatabaseWorkloads
{
    // Initializes the database by creating the models and adding placeholder data.
    public static async Task InitializeDatabaseAsync(this WebApplication app, bool useSqlite)
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
            if (useSqlite)
            {
                // When using SQLite, placeholder data for all models will be inserted by code.
                await dbinit.AddUserAndBusinessDataToContext();
                return;
            }
            // Otherwise, placeholder data will be inserted by code only for user-related models.
            await dbinit.AddUserDataToContext();
        }
    }
}
