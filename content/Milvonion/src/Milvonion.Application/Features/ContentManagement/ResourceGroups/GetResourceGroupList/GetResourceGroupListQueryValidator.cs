using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupList;

/// <summary>
/// Validations of <see cref="GetResourceGroupListQuery"/>.
/// </summary>
public sealed class GetResourceGroupListQueryValidator : AbstractValidator<GetResourceGroupListQuery>
{
    ///<inheritdoc cref="GetResourceGroupListQueryValidator"/>
    public GetResourceGroupListQueryValidator()
    {
    }
}