using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Abstractions;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Development service for development purposes.
/// </summary>
public interface IDevelopmentService : IInterceptable
{
    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    Task<Response> ResetDatabaseAsync();

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    Task<Response> SeedDevelopmentDataAsync();

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    Task<Response<string>> InitDatabaseAsync();

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest);

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest);
}
