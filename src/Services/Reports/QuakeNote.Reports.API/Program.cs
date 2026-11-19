using Microsoft.EntityFrameworkCore;
using QuakeNote.Reports.Core;
using QuakeNote.Reports.Data;
using QuakeNote.Shared.Cache.Extensions;
using QuakeNote.Shared.Messaging.Extensions;
using QuakeNote.Shared.Web.Extensions;
using Serilog;

const string ServiceTitle = "QuakeNote Reports API";

var builder = WebApplication.CreateBuilder(args);

builder.AddQuakeNoteLogging("reports");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

builder.Services.AddReportsData(connectionString);
builder.Services.AddReportsCore();
builder.Services.AddQuakeNoteCache(builder.Configuration);
builder.Services.AddQuakeNoteMessaging(builder.Configuration);
builder.Services.AddQuakeNoteJwtAuth(builder.Configuration);
builder.Services.AddQuakeNoteServiceAuth(builder.Configuration);
builder.Services.AddQuakeNoteControllers();
builder.Services.AddQuakeNoteSwagger(ServiceTitle, "Отчёты пользователей об ощутимых землетрясениях, расширенная анкета MSK-64, история отчётов.");
builder.Services.AddHealthChecks().AddDbContextCheck<ReportsDbContext>("db");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ReportsDbContext>().Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
app.UseQuakeNoteErrorHandling();
app.UseQuakeNoteSwagger(ServiceTitle);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapQuakeNoteHealth();

app.Run();