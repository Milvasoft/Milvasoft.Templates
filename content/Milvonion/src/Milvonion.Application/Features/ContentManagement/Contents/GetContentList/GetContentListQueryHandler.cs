using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentList;

/// <summary>
/// Handles the content list operation.
/// </summary>
/// <param name="resourceGroupRepository"></param>
public class GetContentListQueryHandler(IMilvonionRepositoryBase<Content> resourceGroupRepository) : IInterceptable, IListQueryHandler<GetContentListQuery, ContentListDto>
{
    private readonly IMilvonionRepositoryBase<Content> _resourceGroupRepository = resourceGroupRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<ContentListDto>> Handle(GetContentListQuery request, CancellationToken cancellationToken)
    {
        var response = await _resourceGroupRepository.GetAllAsync(request, projection: ContentListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
