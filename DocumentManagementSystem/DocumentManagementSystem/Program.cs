using DocumentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using DocumentManagementSystem.Options;
using DocumentManagementSystem.Storage;
using DocumentManagementSystem.Repository;
using DocumentManagementSystem.Entity;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();

builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Upload"));
builder.Services.AddScoped<IDocumentRepository<Document>, DocumentRepository>();

builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 11 * 1024 * 1024);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        && context.Request.ContentLength > 11 * 1024 * 1024)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status413PayloadTooLarge,
            Title = "File too large",
            Detail = "The upload exceeds the maximum allowed size of 10 MB."
        });
        return;
    }
    await next();
});
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
