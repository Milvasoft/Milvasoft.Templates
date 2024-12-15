using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentList;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class GetContentListQueryValidator : AbstractValidator<GetContentListQuery>
{
    ///<inheritdoc cref="GetContentListQueryValidator"/>
    public GetContentListQueryValidator()
    {
    }
}