using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.ContentManagementDtos.LanguageDtos;
using Milvonion.Application.Features.Languages.GetLanguageList;
using Milvonion.Application.Features.Languages.UpdateLanguage;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Language endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class LanguagesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets languages.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.LanguageManagement.List)]
    [HttpPatch]
    public async Task<ListResponse<LanguageDto>> GetLanguagesAsync(GetLanguageListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Updates language. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.LanguageManagement.Update)]
    [HttpPut("language")]
    public async Task<Response<int>> UpdateLanguagesAsync(UpdateLanguageCommand request) => await _mediator.Send(request);
}