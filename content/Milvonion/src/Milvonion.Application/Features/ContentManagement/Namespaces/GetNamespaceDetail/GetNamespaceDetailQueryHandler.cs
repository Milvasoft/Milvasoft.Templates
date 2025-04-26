using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ContentManagementDtos.NamespaceDtos;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceDetail;

/// <summary>
/// Handles the contentNamespace detail operation.
/// </summary>
/// <param name="contentNamespaceRepository"></param>
public class GetNamespaceDetailQueryHandler(IMilvonionRepositoryBase<Namespace> contentNamespaceRepository) : IInterceptable, IQueryHandler<GetNamespaceDetailQuery, NamespaceDetailDto>
{
    private readonly IMilvonionRepositoryBase<Namespace> _contentNamespaceRepository = contentNamespaceRepository;

    /// <inheritdoc/>
    public async Task<Response<NamespaceDetailDto>> Handle(GetNamespaceDetailQuery request, CancellationToken cancellationToken)
    {
        var contentNamespace = await _contentNamespaceRepository.GetByIdAsync(request.NamespaceId, projection: NamespaceDetailDto.Projection, cancellationToken: cancellationToken);

        if (contentNamespace == null)
            return Response<NamespaceDetailDto>.Success(contentNamespace, MessageKey.NamespaceNotFound, MessageType.Warning);

        return Response<NamespaceDetailDto>.Success(contentNamespace);
    }
}
