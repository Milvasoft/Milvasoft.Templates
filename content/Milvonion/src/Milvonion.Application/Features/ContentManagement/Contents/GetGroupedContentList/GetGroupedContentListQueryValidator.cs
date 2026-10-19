using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetGroupedContentList;

/// <summary>
/// Validations of <see cref="GetGroupedContentListQuery"/>.
/// </summary>
public sealed class GetGroupedContentListQueryValidator : AbstractValidator<GetGroupedContentListQuery>
{
    ///<inheritdoc cref="GetGroupedContentListQueryValidator"/>
    public GetGroupedContentListQueryValidator()
    {
    }
}