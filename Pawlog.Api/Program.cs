using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Pawlog.Api.Data;
using Pawlog.Api.Services;

const string WebAppCorsPolicy = "WebApp";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PawlogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Pawlog")));
builder.Services.AddSingleton<PhotoStorage>();

builder.Services.AddCors(options =>
    options.AddPolicy(WebAppCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DatabaseSeeder.Seed(scope.ServiceProvider.GetRequiredService<PawlogDbContext>());
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(WebAppCorsPolicy);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(app.Services.GetRequiredService<PhotoStorage>().RootPath),
    RequestPath = PhotoStorage.RequestPath
});

app.UseAuthorization();

app.MapControllers();

app.Run();
