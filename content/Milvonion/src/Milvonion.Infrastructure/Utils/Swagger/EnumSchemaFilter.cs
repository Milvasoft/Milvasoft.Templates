using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvonion.Infrastructure.Utils.Swagger;

/// <summary>
/// View enums strings with values.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            model.Enum.Clear();

            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(name => model.Enum.Add(new OpenApiString($"{Convert.ToInt64(Enum.Parse(context.Type, name))} -> {name}")));
        }
    }
}