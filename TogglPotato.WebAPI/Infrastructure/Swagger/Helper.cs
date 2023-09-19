using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TogglPotato.WebAPI.Infrastructure.Swagger;

public class SwaggerVersionMapping : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        OpenApiPaths pathLists = new OpenApiPaths();
        foreach (KeyValuePair<string, OpenApiPathItem> path in swaggerDoc.Paths)
        {
            pathLists.Add(path.Key.Replace("v{version}", "v1"), path.Value);
        }
        swaggerDoc.Paths = pathLists;
    }
}