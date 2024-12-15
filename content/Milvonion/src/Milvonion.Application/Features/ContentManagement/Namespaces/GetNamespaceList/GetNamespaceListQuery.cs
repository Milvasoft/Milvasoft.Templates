using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ContentManagementDtos.NamespaceDtos;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceList;

/// <summary>
/// Data transfer object for contentNamespace list.
/// </summary>
public record GetNamespaceListQuery : ListRequest, IListRequestQuery<NamespaceListDto>
{
}