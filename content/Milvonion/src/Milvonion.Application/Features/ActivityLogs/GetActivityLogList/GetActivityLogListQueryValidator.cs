using FluentValidation;

namespace Milvonion.Application.Features.ActivityLogs.GetActivityLogList;

/// <summary>
/// Validations of <see cref="GetActivityLogListQuery"/>.
/// </summary>
public sealed class GetActivityLogListQueryValidator : AbstractValidator<GetActivityLogListQuery>
{
    ///<inheritdoc cref="GetActivityLogListQueryValidator"/>
    public GetActivityLogListQueryValidator()
    {
    }
}