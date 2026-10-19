using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Concrete;
using Milvonion.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the ActivityLogs table.
/// </summary>
[Table(TableNames.ActivityLogs)]
[Index(nameof(ActivityDate), IsDescending = [true])]
[Index(nameof(UserName), nameof(ActivityDate), IsDescending = [false, true])]
public class ActivityLog : BaseEntity<int>
{
    /// <summary>
    /// Username of the user performing the activity.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; }

    /// <summary>
    /// Description of the activity performed.
    /// </summary>
    [Required]
    public UserActivity Activity { get; set; }

    /// <summary>
    /// Date and time when the activity occurred.
    /// </summary>
    [Required]
    public DateTimeOffset ActivityDate { get; set; }

    /// <summary>
    /// Id of the record the activity acted on (e.g. the user or role id), for deep-linking from the log. Null when not applicable.
    /// </summary>
    [MaxLength(100)]
    public string RelatedId { get; set; }

    /// <summary>
    /// Optional related detail in JSON format (extra routing/display context for the related record).
    /// </summary>
    public string RelatedData { get; set; }
}
