﻿using FluentValidation;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetGroupedContentList;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class GetGroupedContentListQueryValidator : AbstractValidator<GetGroupedContentListQuery>
{
    ///<inheritdoc cref="GetGroupedContentListQueryValidator"/>
    public GetGroupedContentListQueryValidator()
    {
    }
}