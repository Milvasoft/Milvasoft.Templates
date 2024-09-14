using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the MigrationHistory table.
/// </summary>
[Table(TableNames.MigrationHistory)]
public class MigrationHistory
{
    /// <summary>
    /// Id of migration
    /// </summary>
    [Key]
    public string MigrationId { get; set; }

    /// <summary>
    /// Version of migration tool
    /// </summary>
    public string ProductVersion { get; set; }

    /// <summary>
    /// Migration fully completed or not.
    /// </summary>
    public bool MigrationCompleted { get; set; }
}
