using FluentValidation;

namespace Milvonion.Application.Features.Permissions.GetPermissionList;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetPermissionListQueryValidator : AbstractValidator<GetPermissionListQuery>
{
    ///<inheritdoc cref="GetPermissionListQueryValidator"/>
    public GetPermissionListQueryValidator()
    {
    }
}