﻿using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.RoleDtos;

namespace Milvonion.Application.Features.Roles.GetRoleDetail;

/// <summary>
/// Handles the role detail operation.
/// </summary>
/// <param name="roleRepository"></param>
public class GetRoleDetailQueryHandler(IMilvonionRepositoryBase<Role> roleRepository) : IInterceptable, IQueryHandler<GetRoleDetailQuery, RoleDetailDto>
{
    private readonly IMilvonionRepositoryBase<Role> _roleRepository = roleRepository;

    /// <inheritdoc/>
    public async Task<Response<RoleDetailDto>> Handle(GetRoleDetailQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, null, RoleDetailDto.Projection, cancellationToken: cancellationToken);

        if (role == null)
            return Response<RoleDetailDto>.Success(role, MessageKey.UserNotFound, MessageType.Warning);

        return Response<RoleDetailDto>.Success(role);
    }
}
