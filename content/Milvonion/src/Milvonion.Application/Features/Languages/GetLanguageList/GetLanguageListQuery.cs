using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ContentManagementDtos.LanguageDtos;

namespace Milvonion.Application.Features.Languages.GetLanguageList;

/// <summary>
/// Data transfer object for language list.
/// </summary>
public record GetLanguageListQuery : ListRequest, IListRequestQuery<LanguageDto>
{
}