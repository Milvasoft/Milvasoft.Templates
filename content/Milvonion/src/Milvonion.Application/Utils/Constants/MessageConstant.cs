namespace Milvonion.Application.Utils.Constants;

/// <summary>
/// Represents a class that contains global constants for the application.
/// </summary>
public static class MessageConstant
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string DefaultDataCannotModifyPgCode = "P0001";
    public const string DefaultDataCannotModifyPgMessage = "P0001: Seed records cannot be modified.";
    public const string DuplicateDataViolationPgCode = "23505";
    public const string ForeignKeyViolationPgCode = "20503";
    public const string ExceptionLogTemplate = "Exception raised : {message}";
    public const string S3ConfigurationMissing = "Current provider type is S3 but S3 configuration missing!";
    public const string ABSConfigurationMissing = "Current provider type is ABS but ABS configuration missing!";
    public const string SwaggerAuthMessageTip =
    """
        JWT Authorization header using the Bearer scheme.
        Enter your token in the text input below without Bearer. Bearer will automatically be added.
        Example: '12345abcdef'
    """;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
