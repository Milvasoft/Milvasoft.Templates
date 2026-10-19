using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// A runtime kill switch an admin flips from the management endpoints. When <see cref="IsEnabled"/> is false the
/// feature behind <see cref="Key"/> is disabled for everyone: the endpoints marked with that key answer HTTP 503
/// before their action runs (e.g. an expensive or paid integration is switched off while a quota is hit).
/// Every key in <see cref="FeatureFlagKeys.All"/> gets a row on startup, and a missing row reads as enabled (fail-open).
/// </summary>
[Table(TableNames.FeatureFlags)]
[Index(nameof(Key), IsUnique = true)]
public class FeatureFlag : FullAuditableEntity<int>
{
    /// <summary>Stable key, one of <see cref="FeatureFlagKeys"/>. Unique.</summary>
    [MaxLength(50)]
    public string Key { get; set; }

    /// <summary>Whether the feature is currently on. false = kill switch engaged (feature off for everyone).</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Optional short reason for the switch (e.g. "Quota"). Null when enabled or unset.</summary>
    [MaxLength(200)]
    public string Reason { get; set; }
}

/// <summary>
/// The canonical set of feature flag keys: the single source of truth for what can be switched off. A new key is
/// added here and to <see cref="All"/>; its row is created on the next startup.
/// </summary>
public static class FeatureFlagKeys
{
    /// <summary>Excel exports (one export reads up to the export row limit in a single request).</summary>
    public const string Export = "Export";

    /// <summary>All known keys, in display order.</summary>
    public static readonly string[] All = [Export];
}
