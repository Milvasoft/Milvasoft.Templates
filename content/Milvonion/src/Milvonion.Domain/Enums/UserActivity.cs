using System.ComponentModel;

namespace Milvonion.Domain.Enums;
/// <summary>
/// User activity types.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public enum UserActivity : byte
{
    [Description("User added")]
    CreateUser = 1,

    [Description("User updated")]
    UpdateUser = 2,

    [Description("User deleted")]
    DeleteUser = 3,

    [Description("Role added")]
    CreateRole = 4,

    [Description("Role updated")]
    UpdateRole = 5,

    [Description("Role deleted")]
    DeleteRole = 6,

    [Description("Namespace added")]
    CreateNamespace = 7,

    [Description("Namespace updated")]
    UpdateNamespace = 8,

    [Description("Namespace deleted")]
    DeleteNamespace = 9,

    [Description("Resource Group added")]
    CreateResourceGroup = 10,

    [Description("Resource Group updated")]
    UpdateResourceGroup = 11,

    [Description("Resource Group deleted")]
    DeleteResourceGroup = 12,

    [Description("Content added")]
    CreateContent = 13,

    [Description("Content updated")]
    UpdateContent = 14,

    [Description("Content deleted")]
    DeleteContent = 15,

    [Description("Languages updated")]
    UpdateLanguages = 16,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
