﻿using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceList;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetNamespaceListQueryValidator : AbstractValidator<GetNamespaceListQuery>
{
    ///<inheritdoc cref="GetNamespaceListQueryValidator"/>
    public GetNamespaceListQueryValidator()
    {
    }
}