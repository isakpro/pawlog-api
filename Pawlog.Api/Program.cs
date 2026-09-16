using Microsoft.EntityFrameworkCore;
using Pawlog.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PawlogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Pawlog")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DatabaseSeeder.Seed(scope.ServiceProvider.GetRequiredService<PawlogDbContext>());
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
