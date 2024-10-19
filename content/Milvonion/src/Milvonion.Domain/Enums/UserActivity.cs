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
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
