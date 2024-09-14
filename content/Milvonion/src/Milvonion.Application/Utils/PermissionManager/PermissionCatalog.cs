using Milvasoft.Core.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace Milvonion.Application.Utils.PermissionManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static partial class PermissionCatalog
{
    [Description("Application-wide permissions")]
    public static class App
    {
        [Description("Provides access to the entire system.")]
        public const string SuperAdmin = "App.SuperAdmin";
    }

    [Description("User Management")]
    public static class UserManagement
    {
        [Description("User list permission.")]
        public const string List = "UserManagement.List";

        [Description("User detail view permission")]
        public const string Detail = "UserManagement.Detail";

        [Description("User create permission")]
        public const string Create = "UserManagement.Create";

        [Description("User update permission")]
        public const string Update = "UserManagement.Update";

        [Description("User delete permission")]
        public const string Delete = "UserManagement.Delete";
    }

    [Description("Role Management")]
    public static class RoleManagement
    {
        [Description("Role list permission.")]
        public const string List = "RoleManagement.List";

        [Description("Role detail view permission")]
        public const string Detail = "RoleManagement.Detail";

        [Description("Role create permission")]
        public const string Create = "RoleManagement.Create";

        [Description("Role update permission")]
        public const string Update = "RoleManagement.Update";

        [Description("Role delete permission")]
        public const string Delete = "RoleManagement.Delete";
    }

    [Description("Permission Management")]
    public static class PermissionManagement
    {
        [Description("Permission list permission.")]
        public const string List = "PermissionManagement.List";
    }

    [Description("Activity Log Management")]
    public static class ActivityLogManagement
    {
        [Description("Activity Log list permission.")]
        public const string List = "ActivityLogManagement.List";
    }

    /// <summary>
    /// Gets all permissions in the system as grouped by permission group.
    /// </summary>
    /// <returns> Permission group and group's permissions pair. </returns>
    public static Dictionary<string, List<Permission>> GetPermissionGroups()
    {
        var permissionGroups = new Dictionary<string, List<Permission>>();

        var permissionTypes = typeof(PermissionCatalog).GetNestedTypes().Where(t => !t.IsValueType);

        foreach (var permissionType in permissionTypes)
        {
            var permissions = PermissionBase.GetPermissions(permissionType).ToList();

            permissionGroups.Add(permissionType.Name, permissions);
        }

        return permissionGroups;
    }
}

public static class PermissionBase
{
    public static IEnumerable<Permission> Add<T>() => GetPermissions(typeof(T));

    public static IEnumerable<Permission> GetPermissions(Type type)
    {
        foreach (var field in type.GetFields())
        {
            var permission = new Permission
            {
                Name = field.Name,
                NormalizedName = field.Name.MilvaNormalize(),
                Description = field.GetCustomAttribute<DescriptionAttribute>()?.Description,
                PermissionGroup = type.Name,
                PermissionGroupDescription = type.GetCustomAttribute<DescriptionAttribute>()?.Description
            };

            yield return permission;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member