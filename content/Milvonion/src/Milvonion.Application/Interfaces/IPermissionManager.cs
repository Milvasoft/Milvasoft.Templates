using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Permission manager for managing system permissions.
/// </summary>
public interface IPermissionManager : IInterceptable
{
    /// <summary>
    /// Migrate permissions to the database.
    /// </summary>
    /// <returns></returns>
    Task<Response<string>> MigratePermissionsAsync();
}
