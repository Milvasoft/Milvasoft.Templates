﻿using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;
using Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class CreateBulkContentCommandValidator : AbstractValidator<CreateBulkContentCommand>
{
    ///<inheritdoc cref="CreateContentCommandValidator"/>
    public CreateBulkContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Contents)
            .NotNullOrEmpty(localizer, MessageKey.Content);

        RuleForEach(query => query.Contents)
            .SetValidator(new CreateContentDtoValidator(localizer));
    }
}

/// <summary>
/// Query validations. 
/// </summary>
public class CreateContentDtoValidator : AbstractValidator<CreateContentDto>
{
    ///<inheritdoc cref="CreateContentDtoValidator"/>
    public CreateContentDtoValidator(IMilvaLocalizer localizer)
    {
        RuleFor(dto => dto.Key)
            .NotNullOrEmpty(localizer, MessageKey.GlobalKey);

        RuleFor(dto => dto.Value)
            .NotNullOrEmpty(localizer, MessageKey.GlobalValue);

        RuleFor(dto => dto.LanguageId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Language]]);

        RuleFor(dto => dto.ResourceGroupId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.ResourceGroup]]);

        RuleForEach(query => query.Medias)
            .SetValidator(new UpsertMediaValidator(localizer));
    }
}

/// <summary>
/// Query validations. 
/// </summary>
public class UpsertMediaValidator : AbstractValidator<UpsertMediaDto>
{
    ///<inheritdoc cref="UpsertMediaValidator"/>
    public UpsertMediaValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.MediaAsBase64)
            .Must(MilvonionExtensions.IsValidDataUri)
            .When(q => !string.IsNullOrEmpty(q.MediaAsBase64))
            .WithMessage(localizer[MessageKey.UnsupportedMediaType]);

        RuleFor(query => query.MediaAsBase64)
            .Must(MilvonionExtensions.IsBase64StringHasValidFileExtension)
            .When(q => !string.IsNullOrEmpty(q.MediaAsBase64))
            .WithMessage(localizer[MessageKey.OnlyImageFilesAccepted]);

        RuleFor(query => query.MediaAsBase64)
            .Must(MilvonionExtensions.IsBase64StringValidLength)
            .When(q => !string.IsNullOrEmpty(q.MediaAsBase64))
            .WithMessage(localizer[MessageKey.FileSizeMustLowerThan]);
    }
}