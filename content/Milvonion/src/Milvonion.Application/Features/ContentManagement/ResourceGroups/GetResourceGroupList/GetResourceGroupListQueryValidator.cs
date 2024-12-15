using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupList;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class GetResourceGroupListQueryValidator : AbstractValidator<GetResourceGroupListQuery>
{
    ///<inheritdoc cref="GetResourceGroupListQueryValidator"/>
    public GetResourceGroupListQueryValidator()
    {
    }
}