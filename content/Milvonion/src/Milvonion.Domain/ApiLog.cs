using Milvasoft.Core.EntityBases.Concrete;
using Milvonion.Domain.JsonModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the ApiLogs table.
/// </summary>
[Table(TableNames.ApiLogs)]
public class ApiLog : BaseEntity<int>
{
    /// <summary>
    /// Identifier for the API transaction.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TransactionId { get; set; }

    /// <summary>
    /// Severity level of the API request/response.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Severity { get; set; }

    /// <summary>
    /// Date and time when the API request/response occurred.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Path of the API endpoint.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Path { get; set; }

    /// <summary>
    /// JSON string containing request information.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public RequestInfo RequestInfoJson { get; set; }

    /// <summary>
    /// Information returned by the API in response to the request as json.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public ResponseInfo ResponseInfoJson { get; set; }

    /// <summary>
    /// IP address of the client making the API request.
    /// </summary>
    [MaxLength(50)]
    public string IpAddress { get; set; }

    /// <summary>
    /// Elapsed time in ms.
    /// </summary>
    public long ElapsedMs { get; set; }

    /// <summary>
    /// Username of the user associated with the API request.
    /// </summary>
    [MaxLength(100)]
    public string UserName { get; set; }

    /// <summary>
    /// Exception in request if exists.
    /// </summary>
    public string Exception { get; set; }
}
