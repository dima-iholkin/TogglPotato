using TogglPotato.WebAPI.Endpoints.OrganizeDailyTimeEntries;
using TogglPotato.WebAPI.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container:

// Add Swagger/OpenAPI:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ITogglHttpService, TogglHttpService>();
builder.Services.AddScoped<OrganizeDailyTimeEntriesEndpoint>();

var app = builder.Build();

// Configure the HTTP request pipeline:

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

OrganizeDailyTimeEntriesEndpoint.Map(app);

app.UseHttpsRedirection();

app.Run();