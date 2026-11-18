using Microsoft.EntityFrameworkCore;
using QuakeNote.Auth.Core;
using QuakeNote.Auth.Core.Abstractions;
using QuakeNote.Auth.Data;
using QuakeNote.Shared.Cache.Extensions;
using QuakeNote.Shared.Messaging.Extensions;
using QuakeNote.Shared.Web.Extensions;
using Serilog;

const string ServiceTitle = "QuakeNote Auth API";

var builder = WebApplication.CreateBuilder(args);

builder.AddQuakeNoteLogging("auth");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

builder.Services.AddAuthData(connectionString);
builder.Services.AddAuthCore(builder.Configuration);
builder.Services.AddQuakeNoteCache(builder.Configuration);
builder.Services.AddQuakeNoteMessaging(builder.Configuration);
builder.Services.AddQuakeNoteJwtAuth(builder.Configuration);
builder.Services.AddQuakeNoteControllers();
builder.Services.AddQuakeNoteSwagger(ServiceTitle, "Аутентификация по одноразовому коду, JWT, профиль и настройки пользователя.");
builder.Services.AddHealthChecks().AddDbContextCheck<AuthDbContext>("db");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
    _ = scope.ServiceProvider.GetRequiredService<IJwtService>();
}

app.UseSerilogRequestLogging();
app.UseQuakeNoteErrorHandling();
app.UseQuakeNoteSwagger(ServiceTitle);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapQuakeNoteHealth();

app.Run();