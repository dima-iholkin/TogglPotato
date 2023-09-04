using TogglPotato.WebAPI.Endpoints;
using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.HttpClients;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container:

// Add Swagger/OpenAPI:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<Organizer>();
builder.Services.AddHttpClient<ITogglApiService, TogglApiService>();
builder.Services.AddScoped<OrganizeDailyTimeEntriesEndpoint>();

var app = builder.Build();

// Configure the HTTP request pipeline:

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

EndpointsRouter.Map(app);

app.UseHttpsRedirection();

app.Run();