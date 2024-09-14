using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.UIDtos.PageDtos;

namespace Milvonion.Application.Features.Pages.GetPageAccessibilityForCurrentUser;

/// <summary>
/// Data transfer object for page details.
/// </summary>
public record GetPageAccessibilityForCurrentUserQuery : IQuery<PageDto>
{
    /// <summary>
    /// Page name where you want to access the information.
    /// </summary>
    public string PageName { get; set; }
}
