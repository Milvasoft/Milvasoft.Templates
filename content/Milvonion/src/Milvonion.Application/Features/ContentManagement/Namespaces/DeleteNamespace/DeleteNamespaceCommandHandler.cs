using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.DeleteNamespace;

/// <summary>
/// Handles the deletion of the contentNamespace.
/// </summary>
/// <param name="NamespaceRepository"></param>
[Log]
[UserActivityTrack(UserActivity.DeleteNamespace)]
public record DeleteNamespaceCommandHandler(IMilvonionRepositoryBase<Namespace> NamespaceRepository) : IInterceptable, ICommandHandler<DeleteNamespaceCommand, int>
{
    private readonly IMilvonionRepositoryBase<Namespace> _contentNamespaceRepository = NamespaceRepository;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(DeleteNamespaceCommand request, CancellationToken cancellationToken)
    {
        var contentNamespace = await _contentNamespaceRepository.GetForDeleteAsync(request.NamespaceId, cancellationToken: cancellationToken);

        if (contentNamespace == null)
            return Response<int>.Error(0, MessageKey.NamespaceNotFound);

        await _contentNamespaceRepository.DeleteAsync(contentNamespace, cancellationToken: cancellationToken);

        return Response<int>.Success(request.NamespaceId);
    }
}
