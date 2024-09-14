using System.ComponentModel;

namespace Milvonion.Domain.Enums;
/// <summary>
/// User activity types.
/// </summary>
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
