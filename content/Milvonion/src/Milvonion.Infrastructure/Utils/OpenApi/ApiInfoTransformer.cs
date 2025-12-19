using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Milvonion.Infrastructure.Utils.OpenApi;

/// <summary>
/// Api info transformer to set metadata for the OpenAPI document.
/// </summary>
public sealed class ApiInfoTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1", // GlobalConstant.DefaultApiVersion
            Title = "Milvonion Api",
            Description = "Milvonion api.",
            TermsOfService = new Uri("https://www.milvasoft.com"),
            Contact = new OpenApiContact
            {
                Name = "Milvonion",
                Email = "info@milvasoft.com",
                Url = new Uri("https://www.milvasoft.com")
            }
        };

        return Task.CompletedTask;
    }
}