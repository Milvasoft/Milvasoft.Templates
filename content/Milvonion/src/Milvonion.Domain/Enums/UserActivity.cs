using System.ComponentModel;

namespace Milvonion.Domain.Enums;

/// <summary>
/// User activity types. The numeric value is stored in <c>ActivityLogs</c>, so values are explicit and never reused.
/// </summary>
public enum UserActivity : byte
{
    /// <summary>
    /// User added.
    /// </summary>
    [Description("User added")]
    CreateUser = 0,

    /// <summary>
    /// User updated.
    /// </summary>
    [Description("User updated")]
    UpdateUser = 1,

    /// <summary>
    /// User deleted.
    /// </summary>
    [Description("User deleted")]
    DeleteUser = 2,

    /// <summary>
    /// Role added.
    /// </summary>
    [Description("Role added")]
    CreateRole = 3,

    /// <summary>
    /// Role updated.
    /// </summary>
    [Description("Role updated")]
    UpdateRole = 4,

    /// <summary>
    /// Role deleted.
    /// </summary>
    [Description("Role deleted")]
    DeleteRole = 5,

    /// <summary>
    /// Namespace added.
    /// </summary>
    [Description("Namespace added")]
    CreateNamespace = 6,

    /// <summary>
    /// Namespace updated.
    /// </summary>
    [Description("Namespace updated")]
    UpdateNamespace = 7,

    /// <summary>
    /// Namespace deleted.
    /// </summary>
    [Description("Namespace deleted")]
    DeleteNamespace = 8,

    /// <summary>
    /// Resource Group added.
    /// </summary>
    [Description("Resource Group added")]
    CreateResourceGroup = 9,

    /// <summary>
    /// Resource Group updated.
    /// </summary>
    [Description("Resource Group updated")]
    UpdateResourceGroup = 10,

    /// <summary>
    /// Resource Group deleted.
    /// </summary>
    [Description("Resource Group deleted")]
    DeleteResourceGroup = 11,

    /// <summary>
    /// Content added.
    /// </summary>
    [Description("Content added")]
    CreateContent = 12,

    /// <summary>
    /// Content updated.
    /// </summary>
    [Description("Content updated")]
    UpdateContent = 13,

    /// <summary>
    /// Content deleted.
    /// </summary>
    [Description("Content deleted")]
    DeleteContent = 14,

    /// <summary>
    /// Languages updated.
    /// </summary>
    [Description("Languages updated")]
    UpdateLanguages = 15,

    /// <summary>
    /// Feature flag updated.
    /// </summary>
    [Description("Feature flag updated")]
    UpdateFeatureFlag = 16
}