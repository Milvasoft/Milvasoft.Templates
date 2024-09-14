using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvonion.Infrastructure.Utils.Swagger;

/// <summary>
/// Operation filter to add the requirement of the custom header
/// </summary>
public class RequestHeaderFilter : IOperationFilter
{
    /// <summary>
    /// Applies configuration.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor && !descriptor.ControllerName.StartsWith("Weather"))
        {
            var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");

            if (versionParameter != null)
                operation.Parameters.Remove(versionParameter);

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Description = "The lang iso code of system. (e.g. tr-TR)",
                Required = false
            });
        }
    }
}
