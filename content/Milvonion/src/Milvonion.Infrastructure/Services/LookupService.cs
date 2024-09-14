using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.Interception.Interceptors.Logging;
using Milvonion.Application.Dtos;
using Milvonion.Application.Interfaces;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence.Context;
using System.Reflection;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Lookup service for getting dynamic entity fetch.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="localizer"></param>
public class LookupService(MilvonionDbContext dbContext, IMilvaLocalizer localizer) : ILookupService
{
    private readonly MilvonionDbContext _dbContext = dbContext;
    private readonly IMilvaLocalizer _localizer = localizer;

    /// <summary>
    /// Dynamic entity fetch.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<object>> GetLookupsAsync(LookupRequest request)
    {
        var response = await _dbContext.GetLookupsAsync(request);

        return response;
    }

    /// <summary>
    /// Get enum names as localized.
    /// </summary>
    /// <param name="enumName"></param>
    /// <returns></returns>
    [Log]
    public ListResponse<EnumLookupModel> GetEnumLookups(string enumName)
    {
        if (string.IsNullOrWhiteSpace(enumName))
            return ListResponse<EnumLookupModel>.Error();

        var enumType = GetEnumType(enumName);

        if (enumType == null)
            return ListResponse<EnumLookupModel>.Error();

        var enumValues = Enum.GetValues(enumType);

        var enumUnderlyingType = Enum.GetUnderlyingType(enumType);

        var enumLookups = new List<EnumLookupModel>();

        foreach (var enumValue in enumValues)
        {
            var resourceKey = $"{enumType.Name}.{enumValue}";

            var localizedEnumValue = _localizer[resourceKey];

            var enumActualValue = Convert.ChangeType(enumValue, enumUnderlyingType);

            enumLookups.Add(new EnumLookupModel { Value = enumActualValue, Name = localizedEnumValue });
        }

        return ListResponse<EnumLookupModel>.Success(enumLookups);
    }

    private static Type GetEnumType(string enumName)
    {
        var assemblies = new List<Assembly> { DomainAssembly.Assembly, InfrastructureAssembly.Assembly, DomainAssembly.Assembly };

        foreach (var assembly in assemblies)
        {
            var type = Array.Find(assembly.GetExportedTypes(), t => t.IsEnum && t.Name == enumName);

            if (type == null)
                continue;

            if (type.IsEnum)
                return type;
        }

        return null;
    }
}
