using FluentValidation;

namespace Milvonion.Application.Features.Users.GetUserList;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetUserListQueryValidator : AbstractValidator<GetUserListQuery>
{
    ///<inheritdoc cref="GetUserListQueryValidator"/>
    public GetUserListQueryValidator()
    {
    }
}