using System.Diagnostics;
using System.Diagnostics.Metrics;
using Asp.Versioning.Builder;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TogglPotato.WebAPI.Domain.AppService;
using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.Domain.Validators;
using TogglPotato.WebAPI.Endpoints;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.HttpClients.Retries;
using TogglPotato.WebAPI.Observability;
using TogglPotato.WebAPI.StartupTests;
using TogglPotato.WebAPI.Validators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container:

// Add OpenTelemetry.
string serviceName = "TogglPotato.WebAPI";
string serviceVersion = "1.0.0";
ActivitySource appActivitySource = new ActivitySource(serviceName, serviceVersion);
builder.Services.AddSingleton<ActivitySource>(appActivitySource);
Meter appMeter = new Meter(serviceName, serviceVersion);
builder.Services.AddSingleton<Meter>(appMeter);
builder.Services.AddSingleton<Metrics>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(builder => builder.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(builder =>
    {
        builder.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddSource(serviceName);
    })
    .WithMetrics(builder =>
    {
        builder.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(appMeter.Name)
            .AddConsoleExporter();
    });

// Add Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the API Versioning.
builder.Services.AddApiVersioning();

// Add our custom services.
builder.Services.AddScoped<StartDateValidator>();
builder.Services.AddScoped<GlobalTimeService>();
builder.Services.AddScoped<DailyTotalTimeValidator>();
builder.Services.AddScoped<DailyTimeEntriesOrganizer>();
builder.Services.AddHttpClient<ITogglApiService, TogglApiService>()
    .AddPolicyHandler(DefaultRetryPolicy.GetRetryPolicy());
builder.Services.AddScoped<OrganizeDailyTimeEntriesEndpoint>();

WebApplication app = builder.Build();

// Run the startup tests:
StartupTester.RunTests(app);

// Configure the API Versioning:
IVersionedEndpointRouteBuilder versionedApi = app.NewVersionedApi();
RouteGroupBuilder apiV1 = versionedApi.MapGroup("/api/v{version:apiVersion}").HasApiVersion(1.0);

// Configure the HTTP request pipeline:

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

EndpointsRouter.Map(apiV1);

app.UseHttpsRedirection();

app.Run();

// Make the integration tests work.
public partial class Program { }