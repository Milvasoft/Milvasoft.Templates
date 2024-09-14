﻿using System.Text.Json.Serialization;

namespace Milvonion.Api.Utils;

/// <summary>
/// Model for health check alerts.
/// </summary>
public class DefaultNotification
{
    /// <summary>
    /// Alert message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}