using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Persistence;

/// <summary>
/// The single place that writes the <see cref="LanguagesSeed"/> languages and loads the language list the
/// multi-language layer (<see cref="MultiLanguageManager"/>) reads translations with. Every seeder and the startup
/// migration go through here instead of repeating the insert and the list refresh.
/// </summary>
public static class LanguageSeeder
{
    /// <summary>
    /// Inserts the seed languages that are not in the database yet (matched by code), then refreshes the language
    /// list. Idempotent, so it is safe on an initialized database. On a fresh database it must run after
    /// <c>create_triggers.sql</c>, which starts the language ids at 21.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SeedAsync(MilvonionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingCodes = (await context.Languages.Select(l => l.Code).ToListAsync(cancellationToken)).ToHashSet();

        var missingLanguages = LanguagesSeed.Seed.Where(l => !existingCodes.Contains(l.Code)).Select(l => new Language
        {
            Code = l.Code,
            Name = l.Name,
            IsDefault = l.IsDefault,
            Supported = l.Supported,
        }).ToList();

        if (missingLanguages.Count != 0)
        {
            await context.Languages.AddRangeAsync(missingLanguages, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        await LoadAsync(context, cancellationToken);
    }

    /// <summary>
    /// Loads the languages from the database into <see cref="MultiLanguageManager"/>.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task LoadAsync(MilvonionDbContext context, CancellationToken cancellationToken = default)
    {
        var languages = await context.Languages.ToListAsync(cancellationToken);

        MultiLanguageManager.UpdateLanguagesList([.. languages.Cast<ILanguage>()]);
    }
}
