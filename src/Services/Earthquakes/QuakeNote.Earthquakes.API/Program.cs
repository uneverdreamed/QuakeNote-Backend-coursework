using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuakeNote.Earthquakes.API.GrpcServices;
using QuakeNote.Earthquakes.Core;
using QuakeNote.Earthquakes.Core.Consumers;
using QuakeNote.Earthquakes.Core.Seeding;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Shared.Cache.Extensions;
using QuakeNote.Shared.Grpc.Reports;
using QuakeNote.Shared.Messaging.Extensions;
using QuakeNote.Shared.Web.Auth;
using QuakeNote.Shared.Web.Extensions;
using Serilog;

const string ServiceTitle = "QuakeNote Earthquakes API";

var builder = WebApplication.CreateBuilder(args);

builder.AddQuakeNoteLogging("earthquakes");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
var reportsGrpcUrl = builder.Configuration["Services:ReportsGrpc:Url"]
    ?? throw new InvalidOperationException("Services:ReportsGrpc:Url is not configured.");

builder.Services.AddEarthquakesData(connectionString);
builder.Services.AddEarthquakesCore();
builder.Services.AddQuakeNoteCache(builder.Configuration);
builder.Services.AddQuakeNoteMessaging(
    builder.Configuration,
    configureConsumers: x =>
    {
        x.AddConsumer<ReportMatchedConsumer>();
        x.AddConsumer<ReportUpdatedConsumer>();
    },
    configureEndpoints: (cfg, context) =>
    {
        // Повторы для всех очередей ниже: если Reports временно недоступен, обработчик попробует снова.
        cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

        cfg.AddTopicConsumer<ReportMatchedConsumer>(context,
            "earthquakes.report-matched", "reports.events", "report.matched");
        cfg.AddTopicConsumer<ReportUpdatedConsumer>(context,
            "earthquakes.report-updated", "reports.events", "report.updated");
    });
builder.Services.AddQuakeNoteJwtAuth(builder.Configuration);
builder.Services.AddQuakeNoteServiceAuth(builder.Configuration);
builder.Services.AddQuakeNoteControllers();
builder.Services.AddQuakeNoteSwagger(ServiceTitle,
    "Сейсмические события, станции и показания, агрегированная оценка события и статус верификации.");

builder.Services.AddGrpc(options => options.Interceptors.Add<ServiceSecretInterceptor>());
builder.Services.TryAddTransient<ServiceSecretHandler>();
builder.Services.AddGrpcClient<ReportsGrpc.ReportsGrpcClient>(o => o.Address = new Uri(reportsGrpcUrl))
    .AddHttpMessageHandler<ServiceSecretHandler>();

builder.Services.AddHealthChecks().AddDbContextCheck<EarthquakesDbContext>("db");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<EarthquakesDbContext>().Database.MigrateAsync();

    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Seed:DemoData"))
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync();
}

app.UseSerilogRequestLogging();
app.UseQuakeNoteErrorHandling();
app.UseQuakeNoteSwagger(ServiceTitle);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<EarthquakesGrpcServicer>();
app.MapQuakeNoteHealth();

app.Run();