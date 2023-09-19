using Swashbuckle.AspNetCore.Filters;

namespace TogglPotato.WebAPI.Infrastructure.Swagger;

public class ExampleRequestProvider : IExamplesProvider<ExampleRequestBody>
{
    public ExampleRequestBody GetExamples()
    {
        return new ExampleRequestBody();
    }
}

public class ExampleRequestBody
{
    public string TogglApiKey { get; set; } = "your_Toggl_API_Token_goes_here";
    public string Date { get; set; } = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
}