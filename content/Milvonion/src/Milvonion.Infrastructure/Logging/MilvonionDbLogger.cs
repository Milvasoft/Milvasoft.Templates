using Microsoft.Extensions.Logging;
using Milvasoft.Core.Abstractions;
using Milvonion.Domain;
using System.Text.Json;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Milvonion.Infrastructure.Logging;

/// <summary>
/// Example <see cref="IMilvaLogger"/> implementation.
/// </summary>
/// <param name="loggerFactory"></param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
public class MilvonionDbLogger(ILoggerFactory loggerFactory) : IMilvaLogger
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<MilvonionDbLogger>();

    /// <inheritdoc/>
    public void Log(string logEntry)
    {
        var logObject = JsonSerializer.Deserialize<MethodLog>(logEntry);

        _logger.LogInformation("{TransactionId}{Namespace}{ClassName}{MethodName}{MethodParams}{MethodResult}{ElapsedMs}{UtcLogTime}{CacheInfo}{Exception}{IsSuccess}",
                                   logObject.TransactionId,
                                   logObject.Namespace,
                                   logObject.ClassName,
                                   logObject.MethodName,
                                   logObject.MethodParams,
                                   logObject.MethodResult,
                                   logObject.ElapsedMs,
                                   logObject.UtcLogTime,
                                   logObject.CacheInfo,
                                   logObject.Exception,
                                   logObject.IsSuccess);
    }

    /// <inheritdoc/>
    public async Task LogAsync(string logEntry)
    {
        var logObject = JsonSerializer.Deserialize<MethodLog>(logEntry);

        _logger.LogInformation("{TransactionId}{Namespace}{ClassName}{MethodName}{MethodParams}{MethodResult}{ElapsedMs}{UtcLogTime}{CacheInfo}{Exception}{IsSuccess}",
                                   logObject.TransactionId,
                                   logObject.Namespace,
                                   logObject.ClassName,
                                   logObject.MethodName,
                                   logObject.MethodParams,
                                   logObject.MethodResult,
                                   logObject.ElapsedMs,
                                   logObject.UtcLogTime,
                                   logObject.CacheInfo,
                                   logObject.Exception,
                                   logObject.IsSuccess);

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Debug(string message) => _logger.LogDebug("{Message}", message);

    /// <inheritdoc/>
    public void Debug(string messageTemplate, params object[] propertyValues) => _logger.LogDebug(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Debug(Exception exception, string messageTemplate) => _logger.LogDebug("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogDebug("Exception : {Message} {Values} ", exception.Message, propertyValues);

    /// <inheritdoc/>
    public void Error(string message) => _logger.LogError("{Message}", message);

    /// <inheritdoc/>
    public void Error(string messageTemplate, params object[] propertyValues) => _logger.LogError(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Error(Exception exception, string messageTemplate) => _logger.LogError("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogError("Exception : {Message} {Values} ", exception.Message, propertyValues);

    /// <inheritdoc/>
    public void Fatal(string message) => _logger.LogCritical("{Message}", message);

    /// <inheritdoc/>
    public void Fatal(string messageTemplate, params object[] propertyValues) => _logger.LogCritical(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Fatal(Exception exception, string messageTemplate) => _logger.LogCritical("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogCritical("Exception : {Message} {Values} ", exception.Message, propertyValues);

    /// <inheritdoc/>
    public void Information(string message) => _logger.LogInformation("{Message}", message);

    /// <inheritdoc/>
    public void Information(string messageTemplate, params object[] propertyValues) => _logger.LogInformation(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Information(Exception exception, string messageTemplate) => _logger.LogInformation("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Information(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogInformation("Exception : {Message} {Values} ", exception.Message, propertyValues);

    /// <inheritdoc/>
    public void Verbose(string message) => _logger.LogTrace("{Message}", message);

    /// <inheritdoc/>
    public void Verbose(string messageTemplate, params object[] propertyValues) => _logger.LogTrace(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Verbose(Exception exception, string messageTemplate) => _logger.LogTrace("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogTrace("Exception : {Message} {Values} ", exception.Message, propertyValues);

    /// <inheritdoc/>
    public void Warning(string message) => _logger.LogWarning("{Message}", message);

    /// <inheritdoc/>
    public void Warning(string messageTemplate, params object[] propertyValues) => _logger.LogWarning(messageTemplate, propertyValues);

    /// <inheritdoc/>
    public void Warning(Exception exception, string messageTemplate) => _logger.LogWarning("Exception : {Message} ", exception.Message);

    /// <inheritdoc/>
    public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) => _logger.LogWarning("Exception : {Message} {Values} ", exception.Message, propertyValues);
}
