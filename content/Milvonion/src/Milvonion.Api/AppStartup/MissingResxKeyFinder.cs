using Milvasoft.Core.Utils.Constants;
using Milvonion.Application;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Milvonion.Api.AppStartup;

/// <summary>
/// Finds localization keys that the application uses but are missing from one or more .resx files.
/// Paths are resolved from the solution root (the folder containing <see cref="_solutionFileName"/>), so it works both
/// when the API runs (working directory is the project folder) and in unit tests (working directory is the bin folder).
/// </summary>
public static partial class MissingResxKeyFinder
{
    private const string _solutionFileName = "Milvonion.slnx";
    private const string _sourceFolderName = "src";
    private const string _milvasoftAssemblyPrefix = "Milvasoft";
    private const string _globalPrefix = "Global.";
    private const string _permissionGroupPrefix = "PG.";
    private static readonly string[] _resxFolderPathParts = ["Milvonion.Api", "LocalizationResources", "Resources"];
    private static readonly string[] _excludedSourceFolderNames = ["bin", "obj"];

    // Must match the prefixes in GetActivityLogListQueryHandler, which localizes "Create/Update/Delete{Entity}" as "Global.{Entity}".
    private static readonly string[] _activityPrefixes = ["Create", "Update", "Delete"];

    private static readonly Assembly[] _applicationAssemblies = [DomainAssembly.Assembly, ApplicationAssembly.Assembly, InfrastructureAssembly.Assembly, PresentationAssembly.Assembly];

    /// <summary>
    /// Finds missing keys in .resx files and prints them to the console. Intended for API startup in development:
    /// when the source tree is not available (e.g. running inside a container) the check is skipped with a warning instead of throwing.
    /// </summary>
    /// <returns>Missing keys grouped by .resx file name. Empty when nothing is missing or the check is skipped.</returns>
    public static Dictionary<string, List<string>> FindAndPrintToConsole()
    {
        Dictionary<string, List<string>> missingKeys;

        try
        {
            missingKeys = Find();
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"(Milva Framework Warning) Missing resx key check skipped. {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();

            return [];
        }

        PrintToConsole(missingKeys);

        return missingKeys;
    }

    /// <summary>
    /// Finds missing keys in .resx files. Every file is checked on its own, so a key that exists in one culture but not in another is reported too.
    /// </summary>
    /// <returns>Missing keys grouped by .resx file name. Only files with at least one missing key are included.</returns>
    /// <exception cref="DirectoryNotFoundException">The solution root or the resx folder cannot be found.</exception>
    /// <exception cref="FileNotFoundException">The resx folder does not contain any .resx file.</exception>
    public static Dictionary<string, List<string>> Find()
    {
        var sourceFolderPath = Path.Combine(FindSolutionRootPath(), _sourceFolderName);
        var resxFolderPath = Path.Combine([sourceFolderPath, .. _resxFolderPathParts]);

        var resxKeysByFile = ResxReader.GetKeysByResxFile(resxFolderPath);

        if (resxKeysByFile.Count == 0)
        {
            throw new FileNotFoundException("No resx file found in : " + resxFolderPath);
        }

        var requiredKeys = GetRequiredKeys(sourceFolderPath);

        // Culture parity: a key that exists in any resx file must exist in all of them.
        requiredKeys.UnionWith(resxKeysByFile.Values.SelectMany(keys => keys));

        var missingKeys = new Dictionary<string, List<string>>();

        foreach (var (resxFileName, resxKeys) in resxKeysByFile)
        {
            var missingKeysInFile = requiredKeys.Except(resxKeys).Order(StringComparer.Ordinal).ToList();

            if (missingKeysInFile.Count > 0)
            {
                missingKeys.Add(resxFileName, missingKeysInFile);
            }
        }

        return missingKeys;
    }

    /// <summary>
    /// Collects every key the application asks the localizer for.
    /// </summary>
    private static HashSet<string> GetRequiredKeys(string sourceFolderPath)
    {
        HashSet<string> requiredKeys = [];

        #region MessageKey constants

        requiredKeys.UnionWith(GetConstants(typeof(MessageKey)).Values);

        #endregion

        #region Keys referenced in source code

        requiredKeys.UnionWith(GetKeysFromSourceCode(sourceFolderPath));

        #endregion

        #region Milvasoft framework keys

        requiredKeys.UnionWith(GetFrameworkKeys());

        #endregion

        #region Enum types

        // EnumFormatter, LookupService and ExportService localize enum values as "{EnumName}.{Value}".
        var enumTypes = _applicationAssemblies.Where(a => a != PresentationAssembly.Assembly)
                                              .SelectMany(a => a.GetExportedTypes())
                                              .Where(t => t.IsEnum && t != typeof(UserActivity));

        foreach (var enumType in enumTypes)
        {
            requiredKeys.Add(enumType.Name);

            foreach (var enumValue in Enum.GetNames(enumType))
            {
                requiredKeys.Add($"{enumType.Name}.{enumValue}");
            }
        }

        #endregion

        #region User activities

        foreach (var activityName in Enum.GetNames<UserActivity>())
        {
            var activityPrefix = _activityPrefixes.FirstOrDefault(prefix => activityName.StartsWith(prefix, StringComparison.Ordinal));

            if (activityPrefix != null)
            {
                requiredKeys.Add($"{_globalPrefix}{activityName[activityPrefix.Length..]}");
            }
        }

        #endregion

        #region PermissionCatalog groups and permissions

        // GetPermissionListQueryHandler localizes "PG.{Permission}", "PG.{PermissionGroup}" and "{PermissionGroup}".
        foreach (var (permissionGroup, permissions) in PermissionCatalog.GetPermissionsAndGroups())
        {
            requiredKeys.Add(permissionGroup);
            requiredKeys.Add($"{_permissionGroupPrefix}{permissionGroup}");

            foreach (var permission in permissions)
            {
                requiredKeys.Add($"{_permissionGroupPrefix}{permission.Name}");
            }
        }

        #endregion

        #region Domain entities

        var entityTypes = DomainAssembly.Assembly.GetExportedTypes()
                                                 .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<TableAttribute>() != null);

        foreach (var entityType in entityTypes)
        {
            requiredKeys.Add($"{_globalPrefix}{entityType.Name}");
        }

        #endregion

        return requiredKeys;
    }

    /// <summary>
    /// Scans .cs files under <paramref name="sourceFolderPath"/> for localizer lookups whose key can be resolved statically:
    /// string literals, nameof expressions and <see cref="LocalizerKeys"/> references.
    /// </summary>
    private static HashSet<string> GetKeysFromSourceCode(string sourceFolderPath)
    {
        HashSet<string> keys = [];

        var localizerKeys = GetConstants(typeof(LocalizerKeys));

        foreach (var sourceFile in GetSourceFiles(sourceFolderPath))
        {
            // Full-line comments (including XML docs) are removed so that examples in comments are not reported.
            var content = CommentLineRegex().Replace(File.ReadAllText(sourceFile), string.Empty);

            foreach (Match match in LocalizerLiteralKeyRegex().Matches(content))
            {
                keys.Add(match.Groups["key"].Value);
            }

            foreach (Match match in LocalizerNameofKeyRegex().Matches(content))
            {
                keys.Add(match.Groups["expression"].Value.Split('.')[^1]);
            }

            foreach (Match match in LocalizerKeysReferenceRegex().Matches(content))
            {
                if (localizerKeys.TryGetValue(match.Groups["name"].Value, out var key))
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }

    /// <summary>
    /// Gets .cs files under <paramref name="sourceFolderPath"/>, skipping build output folders.
    /// </summary>
    private static IEnumerable<string> GetSourceFiles(string sourceFolderPath)
        => Directory.EnumerateFiles(sourceFolderPath, "*.cs", SearchOption.AllDirectories)
                    .Where(file => !Path.GetRelativePath(sourceFolderPath, file)
                                        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                        .Any(part => _excludedSourceFolderNames.Contains(part, StringComparer.OrdinalIgnoreCase)));

    /// <summary>
    /// Gets <see cref="LocalizerKeys"/> values that the referenced Milvasoft assemblies actually pass to the localizer,
    /// by matching them against the string literals in those assemblies' metadata.
    /// Values containing whitespace are fallback sentences, not resource keys, so they are ignored.
    /// </summary>
    private static HashSet<string> GetFrameworkKeys()
    {
        var localizerKeys = GetConstants(typeof(LocalizerKeys)).Values.Where(value => !value.Any(char.IsWhiteSpace)).ToHashSet();

        HashSet<string> frameworkKeys = [];

        foreach (var assemblyPath in GetReferencedMilvasoftAssemblyPaths())
        {
            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new PEReader(stream);

            if (!peReader.HasMetadata)
            {
                continue;
            }

            var metadataReader = peReader.GetMetadataReader();

            if (metadataReader.GetHeapSize(HeapIndex.UserString) <= 1)
            {
                continue;
            }

            for (var handle = MetadataTokens.UserStringHandle(1); !handle.IsNil; handle = metadataReader.GetNextHandle(handle))
            {
                var literal = metadataReader.GetUserString(handle);

                if (localizerKeys.Contains(literal))
                {
                    frameworkKeys.Add(literal);
                }
            }
        }

        return frameworkKeys;
    }

    /// <summary>
    /// Walks the reference graph of the application assemblies and returns the file paths of every Milvasoft assembly in it.
    /// </summary>
    private static HashSet<string> GetReferencedMilvasoftAssemblyPaths()
    {
        HashSet<string> assemblyPaths = [];
        HashSet<string> visitedAssemblyNames = [];

        var assemblies = new Queue<Assembly>(_applicationAssemblies);

        while (assemblies.Count > 0)
        {
            foreach (var reference in assemblies.Dequeue().GetReferencedAssemblies())
            {
                if (!reference.Name.StartsWith(_milvasoftAssemblyPrefix, StringComparison.Ordinal) || !visitedAssemblyNames.Add(reference.Name))
                {
                    continue;
                }

                Assembly referencedAssembly;

                try
                {
                    referencedAssembly = Assembly.Load(reference);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }

                assemblies.Enqueue(referencedAssembly);

                if (!string.IsNullOrWhiteSpace(referencedAssembly.Location))
                {
                    assemblyPaths.Add(referencedAssembly.Location);
                }
            }
        }

        return assemblyPaths;
    }

    /// <summary>
    /// Finds the solution root by walking up from the application base directory and the current directory.
    /// </summary>
    private static string FindSolutionRootPath()
    {
        string[] startPaths = [AppContext.BaseDirectory, Directory.GetCurrentDirectory()];

        foreach (var startPath in startPaths)
        {
            var directory = new DirectoryInfo(startPath);

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, _solutionFileName)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException($"'{_solutionFileName}' not found in any parent folder of '{AppContext.BaseDirectory}' or '{Directory.GetCurrentDirectory()}'.");
    }

    /// <summary>
    /// Gets all constant strings of the given class (including nested hierarchy) as name-value pairs.
    /// </summary>
    private static Dictionary<string, string> GetConstants(Type type)
        => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
               .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string) && f.GetRawConstantValue() != null)
               .ToDictionary(f => f.Name, f => f.GetRawConstantValue().ToString());

    /// <summary>
    /// Prints the result of <see cref="Find"/> to the console.
    /// </summary>
    private static void PrintToConsole(Dictionary<string, List<string>> missingKeys)
    {
        if (missingKeys.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("(Milva Framework Info) No missing resx keys found. All good!");
            Console.ResetColor();
            Console.WriteLine();

            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("(Milva Framework Warning) Missing resx keys! The following key values were not found in the resx files:");

        foreach (var (resxFileName, missingKeysInFile) in missingKeys)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{resxFileName} ({missingKeysInFile.Count}):");
            Console.ForegroundColor = ConsoleColor.Red;

            foreach (var missingKey in missingKeysInFile)
            {
                Console.WriteLine($"  {missingKey}");
            }
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Matches localizer lookups with a string literal key. e.g. <c>_milvaLocalizer["Yes"]</c>
    /// </summary>
    [GeneratedRegex(@"[Ll]ocalizer\??\[\s*""(?<key>[^""]+)""")]
    private static partial Regex LocalizerLiteralKeyRegex();

    /// <summary>
    /// Matches localizer lookups with a nameof key. e.g. <c>localizer[nameof(Class.Property)]</c>
    /// </summary>
    [GeneratedRegex(@"[Ll]ocalizer\??\[\s*nameof\((?<expression>[\w\.]+)\)")]
    private static partial Regex LocalizerNameofKeyRegex();

    /// <summary>
    /// Matches Milvasoft <see cref="LocalizerKeys"/> references. e.g. <c>LocalizerKeys.IdentityInvalidEmail</c>
    /// </summary>
    [GeneratedRegex(@"\bLocalizerKeys\.(?<name>\w+)")]
    private static partial Regex LocalizerKeysReferenceRegex();

    /// <summary>
    /// Matches lines that contain only a comment.
    /// </summary>
    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline)]
    private static partial Regex CommentLineRegex();
}

/// <summary>
/// Reads .resx files and extracts key-value pairs.
/// </summary>
public static class ResxReader
{
    /// <summary>
    /// Belirtilen bir .resx dosyasındaki tüm anahtar-değer çiftlerini okur.
    /// </summary>
    /// <param name="resxFilePath">.resx dosyasının tam yolu.</param>
    /// <returns>Anahtar ve değerlerden oluşan bir sözlük.</returns>
    public static Dictionary<string, string> ReadResxFile(string resxFilePath)
    {
        if (!File.Exists(resxFilePath))
            throw new FileNotFoundException("Cannot find resx file.", resxFilePath);

        var keyValuePairs = new Dictionary<string, string>();

        var xdoc = XDocument.Load(resxFilePath);

        // Read all data elements
        foreach (var dataElement in xdoc.Descendants("data"))
        {
            var key = dataElement.Attribute("name")?.Value;
            var value = dataElement.Element("value")?.Value;

            if (key != null && value != null)
            {
                keyValuePairs[key] = value;
            }
        }

        return keyValuePairs;
    }

    /// <summary>
    /// Bir klasördeki tüm .resx dosyalarını okur ve anahtarları toplar.
    /// </summary>
    /// <param name="folderPath">.resx dosyalarının bulunduğu klasör.</param>
    /// <returns>Anahtarların bir kümesi.</returns>
    public static HashSet<string> GetAllKeysFromResxFiles(string folderPath)
        => GetKeysByResxFile(folderPath).Values.SelectMany(keys => keys).ToHashSet();

    /// <summary>
    /// Bir klasördeki tüm .resx dosyalarını okur ve anahtarları dosya bazında döner.
    /// </summary>
    /// <param name="folderPath">.resx dosyalarının bulunduğu klasör.</param>
    /// <returns>Dosya adı (klasöre göre göreli yol) ve o dosyadaki anahtarlardan oluşan bir sözlük.</returns>
    public static Dictionary<string, HashSet<string>> GetKeysByResxFile(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException("Folder not found : " + folderPath);
        }

        var keysByFile = new Dictionary<string, HashSet<string>>();

        foreach (var resxFile in Directory.GetFiles(folderPath, "*.resx", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            keysByFile.Add(Path.GetRelativePath(folderPath, resxFile), [.. ReadResxFile(resxFile).Keys]);
        }

        return keysByFile;
    }
}
