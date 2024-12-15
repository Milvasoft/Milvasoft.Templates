using Mapster;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.CreateNamespace;

/// <summary>
/// Handles the creation of the contentNamespace.
/// </summary>
/// <param name="NamespaceRepository"></param>
[Log]
[UserActivityTrack(UserActivity.CreateNamespace)]
public record CreateNamespaceCommandHandler(IMilvonionRepositoryBase<Namespace> NamespaceRepository) : IInterceptable, ICommandHandler<CreateNamespaceCommand, int>
{
    private readonly IMilvonionRepositoryBase<Namespace> _contentNamespaceRepository = NamespaceRepository;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(CreateNamespaceCommand request, CancellationToken cancellationToken)
    {
        var contentNamespace = request.Adapt<Namespace>();

        contentNamespace.Slug = request.Name.ToLowerAndNonSpacingUnicode();

        await _contentNamespaceRepository.AddAsync(contentNamespace, cancellationToken);

        return Response<int>.Success(contentNamespace.Id);
    }
}
