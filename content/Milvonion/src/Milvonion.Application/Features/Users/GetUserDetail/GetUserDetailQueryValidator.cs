using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Users.GetUserDetail;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetUserDetailQueryValidator : AbstractValidator<GetUserDetailQuery>
{
    ///<inheritdoc cref="GetUserDetailQueryValidator"/>
    public GetUserDetailQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.UserId)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.User]]);
    }
}