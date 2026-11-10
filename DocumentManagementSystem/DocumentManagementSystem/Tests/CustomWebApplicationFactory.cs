using DocumentManagementSystem.Data;
using DocumentManagementSystem.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementSystem.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    public string StoragePath { get; } =
        Path.Combine(Path.GetTempPath(), "dms-tests-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with SQLite in-memory
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            _connection.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            // Point storage at a temp folder (absolute path works with Path.Combine)
            services.PostConfigure<StorageOptions>(o => o.RootPath = StoragePath);

            // Create the schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
            if (Directory.Exists(StoragePath)) Directory.Delete(StoragePath, true);
        }
    }
}