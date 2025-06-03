using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Abstractions;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Developer service.
/// </summary>
public interface IDeveloperService : IInterceptable
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
    /// Seeds fake data.
    /// </summary>
    /// <param name="sameData"></param>
    /// <param name="locale"></param>
    /// <returns></returns>
    Task<Response> SeedFakeDataAsync(bool sameData = true, string locale = "tr");

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    Task<Response<string>> InitDatabaseAsync();

    /// <summary>
    /// Resets ui related data.
    /// </summary>
    /// <returns></returns>
    Task<Response> ResetUIRelatedDataAsync();

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

    /// <summary>
    /// Exports existing data to a JSON file.
    /// </summary>
    /// <returns></returns>
    Task<Response> ExportExistingDataAsync();

    /// <summary>
    /// Imports existing data.
    /// </summary>
    /// <returns></returns>
    Task<Response> ImportExistingDataAsync();
}
