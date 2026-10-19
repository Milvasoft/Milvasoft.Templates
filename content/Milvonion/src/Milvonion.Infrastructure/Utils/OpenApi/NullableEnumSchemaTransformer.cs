using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Milvonion.Infrastructure.Utils.OpenApi;

/// <summary>
/// Gives an enum schema its "Value -> Name" list when the Milvasoft enum transformer left it out. That transformer only
/// recognizes a plain enum type; when the enum's shared component schema is first built from a nullable property
/// (<c>UserType?</c>, <c>NotificationType?</c>...), it stays a bare integer and the clients' generated types lose the
/// values. The list is written in the same format, so every enum reads alike.
/// </summary>
public class NullableEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        var enumType = type.IsEnum ? type : Nullable.GetUnderlyingType(type);

        if (enumType == null || !enumType.IsEnum || schema.Enum is { Count: > 0 })
            return Task.CompletedTask;

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum = [.. Enum.GetValues(enumType)
                              .Cast<object>()
                              .Select(value => (JsonNode)JsonValue.Create($"{Convert.ToInt64(value)} -> {Enum.GetName(enumType, value)}"))];

        return Task.CompletedTask;
    }
}
