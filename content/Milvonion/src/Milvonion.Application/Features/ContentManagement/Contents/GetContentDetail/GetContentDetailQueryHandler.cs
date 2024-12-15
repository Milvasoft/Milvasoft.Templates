using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentDetail;

/// <summary>
/// Handles the content detail operation.
/// </summary>
/// <param name="resourceGroupRepository"></param>
public class GetContentDetailQueryHandler(IMilvonionRepositoryBase<Content> resourceGroupRepository) : IInterceptable, IQueryHandler<GetContentDetailQuery, ContentDetailDto>
{
    private readonly IMilvonionRepositoryBase<Content> _resourceGroupRepository = resourceGroupRepository;

    /// <inheritdoc/>
    public async Task<Response<ContentDetailDto>> Handle(GetContentDetailQuery request, CancellationToken cancellationToken)
    {
        var resourceGroup = await _resourceGroupRepository.GetByIdAsync(request.ContentId, null, ContentDetailDto.Projection, cancellationToken: cancellationToken);

        if (resourceGroup == null)
            return Response<ContentDetailDto>.Success(resourceGroup, MessageKey.ContentNotFound, MessageType.Warning);

        return Response<ContentDetailDto>.Success(resourceGroup);
    }
}
