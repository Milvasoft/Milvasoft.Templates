using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class GetContentQueryValidator : AbstractValidator<GetContentQuery>
{
    ///<inheritdoc cref="GetContentQueryValidator"/>
    public GetContentQueryValidator()
    {
    }
}