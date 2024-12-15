using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ContentManagementDtos.LanguageDtos;

namespace Milvonion.Application.Features.Languages.GetLanguageList;

/// <summary>
/// Handles the language list operation.
/// </summary>
/// <param name="languageRepository"></param>
public class GetLanguageListQueryHandler(IMilvonionRepositoryBase<Language> languageRepository) : IInterceptable, IListQueryHandler<GetLanguageListQuery, LanguageDto>
{
    private readonly IMilvonionRepositoryBase<Language> _languageRepository = languageRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<LanguageDto>> Handle(GetLanguageListQuery request, CancellationToken cancellationToken)
    {
        var response = await _languageRepository.GetAllAsync(request, null, LanguageDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
