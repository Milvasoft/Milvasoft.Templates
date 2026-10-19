using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Types.Classes;
using Moq;

namespace Milvonion.UnitTests.TestSupport;

/// <summary>
/// Builds an <see cref="IMilvaLocalizer"/> double whose indexer echoes the key (and formats arguments as
/// "key|arg1,arg2"), so tests can assert which resource key a component resolved without loading resx files.
/// </summary>
public static class FakeLocalizer
{
    /// <summary>
    /// Creates a localizer that returns the key itself for <c>localizer[key]</c> (and for an empty argument list)
    /// and "key|a,b" for <c>localizer[key, a, b]</c>. Optional <paramref name="overrides"/> replace the value for given keys.
    /// </summary>
    public static Mock<IMilvaLocalizer> Create(Dictionary<string, string> overrides = null)
    {
        var mock = new Mock<IMilvaLocalizer>(MockBehavior.Loose);

        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedValue(key, Resolve(key, overrides)));

        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) =>
            {
                var flat = Flatten(args).ToList();

                return new LocalizedValue(key, flat.Count == 0 ? Resolve(key, overrides) : Resolve(key, overrides) + "|" + string.Join(",", flat));
            });

        return mock;
    }

    // A caller may hand the whole object[] as one argument (params wrapping); flatten one level so the
    // formatted value lists the actual values either way.
    private static IEnumerable<object> Flatten(object[] args)
    {
        foreach (var arg in args ?? [])
        {
            if (arg is object[] nested)
            {
                foreach (var inner in nested)
                    yield return inner;
            }
            else
                yield return arg;
        }
    }

    private static string Resolve(string key, Dictionary<string, string> overrides)
        => overrides != null && overrides.TryGetValue(key, out var value) ? value : key;
}
