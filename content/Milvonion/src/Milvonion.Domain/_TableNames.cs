namespace Milvonion.Domain;

public static class TableNames
{
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoleRelations = nameof(UserRoleRelations);
    public const string Permissions = nameof(Permissions);
    public const string RolePermissionRelations = nameof(RolePermissionRelations);
    public const string ActivityLogs = nameof(ActivityLogs);
    public const string ApiLogs = nameof(ApiLogs);
    public const string MethodLogs = nameof(MethodLogs);
    public const string UserSessions = nameof(UserSessions);
    public const string MenuItems = nameof(MenuItems);
    public const string MenuItemTranslations = nameof(MenuItemTranslations);
    public const string MenuGroups = nameof(MenuGroups);
    public const string MenuGroupTranslations = nameof(MenuGroupTranslations);
    public const string Pages = nameof(Pages);
    public const string PageActions = nameof(PageActions);
    public const string PageActionTranslations = nameof(PageActionTranslations);
    public const string MigrationHistory = "_MigrationHistory";
}