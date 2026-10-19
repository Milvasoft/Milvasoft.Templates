using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Dtos.PermissionDtos;

namespace Milvonion.Application.Features.Permissions.GetPermissionList;

/// <summary>
/// Handles the permission list operation.
/// </summary>
/// <param name="permissionRepository"></param>
/// <param name="milvaLocalizer"></param>
public class GetPermissionListQueryHandler(IMilvonionRepositoryBase<Permission> permissionRepository, IMilvaLocalizer milvaLocalizer) : IInterceptable, IListQueryHandler<GetPermissionListQuery, PermissionListDto>
{
    // Permissions are a fixed, code-defined catalog consumed as a whole (role editor): one page holds all of it with room
    // to grow, and no request can ask for more.
    private const int _catalogRowCount = 1000;

    private readonly IMilvonionRepositoryBase<Permission> _permissionRepository = permissionRepository;
    private readonly IMilvaLocalizer _milvaLocalizer = milvaLocalizer;

    /// <inheritdoc/>
    public async Task<ListResponse<PermissionListDto>> Handle(GetPermissionListQuery request, CancellationToken cancellationToken)
    {
        // A caller that omits paging still receives the full list, and a larger page is clamped to the catalog size.
        request.PageNumber ??= 1;
        request.RowCount = request.RowCount.ClampRowCount(_catalogRowCount, _catalogRowCount);

        var response = await _permissionRepository.GetAllAsync(request, projection: PermissionListDto.Projection, cancellationToken: cancellationToken);

        response.Data.ForEach(response =>
        {
            response.Description = _milvaLocalizer[$"PG.{response.Name}", _milvaLocalizer[response.PermissionGroup]];
            response.PermissionGroupDescription = _milvaLocalizer[$"PG.{response.PermissionGroup}"];
        });

        return response;
    }
}
