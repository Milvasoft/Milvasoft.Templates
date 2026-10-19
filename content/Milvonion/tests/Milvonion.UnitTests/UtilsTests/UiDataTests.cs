using FluentAssertions;
using Milvonion.Api.AppStartup;
using Milvonion.Application.Utils.Models;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.UnitTests.TestSupport;
using System.Text.Json;
using System.Xml.Linq;

namespace Milvonion.UnitTests.UtilsTests;

/// <summary>
/// The menu and page seed (<c>StaticFiles/JSON/ui_data.json</c>) is plain JSON, so nothing else checks that its permission
/// names exist in <see cref="PermissionCatalog"/>: a typo hides a menu item from everybody but the super admin.
/// </summary>
[Trait("Utils Unit Tests", "UI seed data (menu and pages).")]
public class UiDataTests
{
    /// <summary>
    /// Permission groups of <see cref="PermissionCatalog"/> that have no screen of their own, with the reason. Every other
    /// group must have a page and a menu item that leads to it.
    /// </summary>
    private static readonly Dictionary<string, string> _groupsWithoutPage = new()
    {
        [nameof(PermissionCatalog.App)] = "SuperAdmin is not a screen; every menu item and page lists it next to its own permission.",
        [nameof(PermissionCatalog.PermissionManagement)] = "The permission list is read by the create and edit forms of the RoleManagement page.",
        [nameof(PermissionCatalog.InternalNotificationManagement)] = "The handlers exist but no controller exposes them; add the page and the menu item together with the controller.",
    };

    [Fact]
    public void EveryPermissionName_ShouldExistInPermissionCatalog()
    {
        // Arrange
        var data = Load();

        var catalog = PermissionCatalog.GetPermissionsAndGroups();
        var known = catalog.SelectMany(group => group.Value.Select(permission => $"{group.Key}.{permission.Name}")).Concat(catalog.Keys).ToHashSet();

        // Act
        var used = MenuItems(data.MenuItems).SelectMany(item => (item.PermissionOrGroupNames ?? []).Select(name => (Owner: $"menu item {item.Id}", Name: name)))
                                            .Concat(data.Pages.SelectMany(page => PagePermissions(page).Select(name => (Owner: $"page {page.Name}", Name: name))));

        var violations = used.Where(u => !known.Contains(u.Name)).Select(u => $"{u.Owner}: {u.Name}");

        // Assert
        violations.Should().BeEmpty("every permission of the menu and the pages must be a PermissionCatalog permission or group");
    }

    [Fact]
    public void Ids_ShouldBeUniqueAndPagesShouldExist()
    {
        // Arrange
        var data = Load();
        var items = MenuItems(data.MenuItems).ToList();
        var pageNames = data.Pages.Select(p => p.Name).ToList();

        // Act
        var duplicateItemIds = items.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key);
        var duplicatePageNames = pageNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key);
        var missingPages = items.Where(i => i.PageName != null && !pageNames.Contains(i.PageName)).Select(i => $"menu item {i.Id}: {i.PageName}");

        // Assert
        duplicateItemIds.Should().BeEmpty();
        duplicatePageNames.Should().BeEmpty();
        missingPages.Should().BeEmpty();
    }

    [Fact]
    public void EveryPage_ShouldHaveALocalizedName()
    {
        // Arrange: UIService localizes a page as "UI.{Name}".
        var data = Load();

        var resxPath = Path.Combine(ApiFolder(), "LocalizationResources", "Resources", "SharedResource.tr-TR.resx");
        var resxKeys = XDocument.Load(resxPath).Root.Elements("data").Select(e => e.Attribute("name").Value).ToHashSet();

        // Act
        var missing = data.Pages.Select(p => $"UI.{p.Name}").Where(key => !resxKeys.Contains(key));

        // Assert
        missing.Should().BeEmpty();
    }

    [Fact]
    public void EveryManagementGroup_ShouldHaveAPageAndAMenuItem()
    {
        // Arrange: a page is named after its permission group, and its menu item is shown with one of the group's permissions.
        var data = Load();
        var items = MenuItems(data.MenuItems).ToList();
        var pageNames = data.Pages.Select(p => p.Name).ToHashSet();
        var groups = PermissionCatalog.GetPermissionsAndGroups().Keys.ToList();

        // Act
        var missing = groups.Except(_groupsWithoutPage.Keys)
                            .Where(group => !pageNames.Contains(group)
                                            || !items.Exists(i => i.PageName == group && (i.PermissionOrGroupNames ?? []).Exists(name => name == group || name.StartsWith($"{group}."))));

        // Assert
        missing.Should().BeEmpty("every permission group needs a page and a menu item, or an entry with its reason in _groupsWithoutPage");
        _groupsWithoutPage.Keys.Should().BeSubsetOf(groups, "an exception must name an existing permission group");
    }

    private static IEnumerable<MenuItemModel> MenuItems(IEnumerable<MenuItemModel> items)
        => items.SelectMany(item => new[] { item }.Concat(MenuItems(item.Children ?? [])));

    private static IEnumerable<string> PagePermissions(PageModel page)
        => (page.CreatePermissions ?? []).Concat(page.EditPermissions ?? [])
                                         .Concat(page.DetailPermissions ?? [])
                                         .Concat(page.DeletePermissions ?? [])
                                         .Concat((page.AdditionalActions ?? []).SelectMany(action => action.Permissions ?? []));

    private static string ApiFolder()
    {
        if (SourceCode.SolutionRoot == null)
            Assert.Skip("The sources are not available.");

        return Path.Combine(SourceCode.SolutionRoot, "src", typeof(IApiMarker).Assembly.GetName().Name);
    }

    /// <summary>
    /// Reads the seed the way DatabaseMigrator.SeedUIRelatedDataAsync does (case insensitive).
    /// </summary>
    private static UISeedModel Load()
    {
        var json = File.ReadAllText(Path.Combine(ApiFolder(), "StaticFiles", "JSON", "ui_data.json"));

        return JsonSerializer.Deserialize<UISeedModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
