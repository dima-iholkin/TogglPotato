using TogglPotato.WebAPI.Endpoints.OrganizeTheDailyTimeEntries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("toggl_api", (client) => {
    client.BaseAddress = new Uri("https://api.track.toggl.com");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

OrganizeTheDailyTimeEntries_Endpoint.Map(app);

app.UseHttpsRedirection();

app.Run();