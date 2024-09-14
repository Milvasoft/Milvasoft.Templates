using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvonion.Domain.UI;
using System.Text.Json.Serialization;

namespace Milvonion.Domain.JsonModels;

/// <summary>
/// Json model for menu group translations.
/// </summary>
public class PageActionTranslation : ITranslationEntityWithIntKey<PageAction>
{
    /// <summary>
    /// Gets or sets the menu item name.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Language id.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Related entity id.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Unused property. Because of jsonb.
    /// </summary>
    [JsonIgnore]
    public PageAction Entity { get; set; }
}
