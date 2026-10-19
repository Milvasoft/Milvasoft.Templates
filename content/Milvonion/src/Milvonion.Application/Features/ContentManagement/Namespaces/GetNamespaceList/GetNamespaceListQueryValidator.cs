using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceList;

/// <summary>
/// Validations of <see cref="GetNamespaceListQuery"/>.
/// </summary>
public sealed class GetNamespaceListQueryValidator : AbstractValidator<GetNamespaceListQuery>
{
    ///<inheritdoc cref="GetNamespaceListQueryValidator"/>
    public GetNamespaceListQueryValidator()
    {
    }
}