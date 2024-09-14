using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.RoleDtos;

namespace Milvonion.Application.Features.Roles.GetRoleList;

/// <summary>
/// Handles the role list operation.
/// </summary>
/// <param name="roleRepository"></param>
public class GetRoleListQueryHandler(IMilvonionRepositoryBase<Role> roleRepository) : IInterceptable, IListQueryHandler<GetRoleListQuery, RoleListDto>
{
    private readonly IMilvonionRepositoryBase<Role> _roleRepository = roleRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<RoleListDto>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
    {
        var response = await _roleRepository.GetAllAsync(request, null, RoleListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
