using Asp.Versioning.Builder;
using TogglPotato.WebAPI.Endpoints;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.HttpClients;
using TogglPotato.WebAPI.Domain.Validators;
using TogglPotato.WebAPI.Domain.Services;
using TogglPotato.WebAPI.Domain.AppService;
using TogglPotato.WebAPI.Validators;
using TogglPotato.WebAPI.HttpClients.Retries;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container:

// Add Swagger/OpenAPI:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add API Versioning:
builder.Services.AddApiVersioning();

// Add custom services:
builder.Services.AddScoped<StartDateValidator>();
builder.Services.AddScoped<GlobalTimeService>();
builder.Services.AddScoped<DailyTotalTimeValidator>();
builder.Services.AddScoped<DailyTimeEntriesOrganizer>();
builder.Services.AddHttpClient<ITogglApiService, TogglApiService>()
    .AddPolicyHandler(DefaultRetryPolicy.GetRetryPolicy());
builder.Services.AddScoped<OrganizeDailyTimeEntriesEndpoint>();

WebApplication app = builder.Build();

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