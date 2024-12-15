using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.ContentManagementDtos.NamespaceDtos;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceDetail;

/// <summary>
/// Data transfer object for contentNamespace details.
/// </summary>
public record GetNamespaceDetailQuery : IQuery<NamespaceDetailDto>
{
    /// <summary>
    /// Namespace id to access details.
    /// </summary>
    public int NamespaceId { get; set; }
}
