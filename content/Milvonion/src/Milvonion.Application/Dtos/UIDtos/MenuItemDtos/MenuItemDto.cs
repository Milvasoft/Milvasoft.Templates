using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvonion.Domain.UI;

namespace Milvonion.Application.Dtos.UIDtos.MenuItemDtos;

/// <summary>
/// Data transfer object for user list.
/// </summary>
[Translate]
[ExcludeFromMetadata]
public class MenuItemDto : MilvonionBaseDto<int>
{
    /// <summary>
    /// Name of menu item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Frontend page url for navigate to on click.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Frontend page name for navigate to on click.
    /// </summary>
    public string PageName { get; set; }

    /// <summary>
    /// Related parent menu item id.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Menu item group information.
    /// </summary>
    public NameIntNavigationDto Group { get; set; }

    /// <summary>
    /// Children items of this menu items.
    /// </summary>
    public List<MenuItemDto> Childrens { get; set; }

    /// <summary>
    /// Projection expression for mapping MenuItem entity to MenuItem.
    /// </summary>
    public static Func<MenuItem, MenuItemDto> Projection(IMultiLanguageManager multiLanguageManager)
    {
        var menuItemNameLangExpression = multiLanguageManager.CreateTranslationMapExpression<MenuItem, MenuItemDto, MenuItemTranslation>(i => i.Name).Compile();
        var groupNameLangExpression = multiLanguageManager.CreateTranslationMapExpression<MenuGroup, NameIntNavigationDto, MenuGroupTranslation>(i => i.Name).Compile();

        return u => new MenuItemDto
        {
            Id = u.Id,
            Name = menuItemNameLangExpression(u),
            ParentId = u.ParentId,
            PageName = u.PageName,
            Url = u.Url,
            Group = new NameIntNavigationDto
            {
                Id = u.Group.Id,
                Name = groupNameLangExpression(u.Group)
            },
            Childrens = u.Childrens?.Select(Projection(multiLanguageManager)).ToList()
        };
    }
}