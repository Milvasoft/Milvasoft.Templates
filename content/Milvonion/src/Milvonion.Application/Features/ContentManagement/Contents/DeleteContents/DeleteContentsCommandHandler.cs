using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Helpers;
using Milvasoft.Interception.Interceptors.Logging;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Contents.DeleteContents;

/// <summary>
/// Handles the deletion of the content.
/// </summary>
/// <param name="ContentRepository"></param>
[Log]
[UserActivityTrack(UserActivity.DeleteContent)]
public record DeleteContentsCommandHandler(IMilvonionRepositoryBase<Content> ContentRepository) : IInterceptable, ICommandHandler<DeleteContentsCommand, List<int>>
{
    private readonly IMilvonionRepositoryBase<Content> _contentRepository = ContentRepository;

    /// <inheritdoc/>
    public async Task<Response<List<int>>> Handle(DeleteContentsCommand request, CancellationToken cancellationToken)
    {
        var contents = await _contentRepository.GetForDeleteAsync(condition: c => request.ContentIdList.Contains(c.Id), tracking: false, cancellationToken: cancellationToken);

        if (contents.IsNullOrEmpty())
            return Response<List<int>>.Error([], MessageKey.ContentNotFound);

        await _contentRepository.DeleteAsync(contents, cancellationToken: cancellationToken);

        return Response<List<int>>.Success(request.ContentIdList);
    }
}
