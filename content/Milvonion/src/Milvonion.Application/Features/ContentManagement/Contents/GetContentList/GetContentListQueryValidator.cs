using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentList;

/// <summary>
/// Validations of <see cref="GetContentListQuery"/>.
/// </summary>
public sealed class GetContentListQueryValidator : AbstractValidator<GetContentListQuery>
{
    ///<inheritdoc cref="GetContentListQueryValidator"/>
    public GetContentListQueryValidator()
    {
    }
}