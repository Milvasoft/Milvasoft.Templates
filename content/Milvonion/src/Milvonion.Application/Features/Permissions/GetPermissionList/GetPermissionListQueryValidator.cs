using FluentValidation;

namespace Milvonion.Application.Features.Permissions.GetPermissionList;

/// <summary>
/// Validations of <see cref="GetPermissionListQuery"/>.
/// </summary>
public sealed class GetPermissionListQueryValidator : AbstractValidator<GetPermissionListQuery>
{
    ///<inheritdoc cref="GetPermissionListQueryValidator"/>
    public GetPermissionListQueryValidator()
    {
    }
}