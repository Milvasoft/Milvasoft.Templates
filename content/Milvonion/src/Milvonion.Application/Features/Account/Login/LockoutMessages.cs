using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Account.Login;

/// <summary>
/// Builds the localized "account locked" message from a lockout end, so every login path words the lockout the same way.
/// </summary>
public static class LockoutMessages
{
    /// <summary>
    /// Returns the localized lockout message: <see cref="MessageKey.LockedIndefinite"/> for an indefinite
    /// (or very long) lockout, otherwise <see cref="MessageKey.Locked"/> with the remaining duration.
    /// </summary>
    /// <param name="localizer"></param>
    /// <param name="lockoutEnd"></param>
    /// <returns></returns>
    public static string Build(IMilvaLocalizer localizer, DateTimeOffset lockoutEnd)
    {
        // Indefinite or a very long lockout: no meaningful "try again in" duration.
        if (lockoutEnd.Year >= 9999 || (lockoutEnd - DateTimeOffset.UtcNow).TotalDays > 365)
            return localizer[MessageKey.LockedIndefinite];

        var remaining = lockoutEnd - DateTimeOffset.UtcNow;

        string durationText;

        // Total values, not the components: a 25 hour lockout is "25 hours", not "1 hours".
        if (remaining.TotalHours >= 1)
            durationText = localizer[MessageKey.Hours, (int)remaining.TotalHours];
        else if (remaining.TotalMinutes >= 1)
            durationText = localizer[MessageKey.Minutes, (int)remaining.TotalMinutes];
        else
            durationText = localizer[MessageKey.Seconds, Math.Max(1, (int)remaining.TotalSeconds)];

        return localizer[MessageKey.Locked, durationText];
    }
}
