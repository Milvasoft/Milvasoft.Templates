using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Milvonion.Application.Dtos.ContentManagementDtos.LanguageDtos;

/// <summary>
/// Language model.
/// </summary>
public class LanguageDto : ILanguage
{
    /// <summary>
    /// Language id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the language.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Iso code of the language.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Determines whether the language is supported or not.
    /// </summary>
    public bool Supported { get; set; }

    /// <summary>
    /// Determines whether the language is default or not.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Projection expression for mapping Language entity to LanguageDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<Language, LanguageDto>> Projection { get; } = n => new LanguageDto
    {
        Id = n.Id,
        Name = n.Name,
        Code = n.Code,
        Supported = n.Supported,
        IsDefault = n.IsDefault,
    };
}
