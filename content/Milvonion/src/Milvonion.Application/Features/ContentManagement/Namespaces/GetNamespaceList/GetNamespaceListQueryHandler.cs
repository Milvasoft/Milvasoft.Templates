using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ContentManagementDtos.NamespaceDtos;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceList;

/// <summary>
/// Handles the contentNamespace list operation.
/// </summary>
/// <param name="contentNamespaceRepository"></param>
public class GetNamespaceListQueryHandler(IMilvonionRepositoryBase<Namespace> contentNamespaceRepository) : IInterceptable, IListQueryHandler<GetNamespaceListQuery, NamespaceListDto>
{
    private readonly IMilvonionRepositoryBase<Namespace> _contentNamespaceRepository = contentNamespaceRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<NamespaceListDto>> Handle(GetNamespaceListQuery request, CancellationToken cancellationToken)
    {
        var response = await _contentNamespaceRepository.GetAllAsync(request, projection: NamespaceListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
