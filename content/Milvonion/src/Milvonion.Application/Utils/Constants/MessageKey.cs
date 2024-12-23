﻿namespace Milvonion.Application.Utils.Constants;

/// <summary>
/// Represents a class that contains global constants for the application.
/// </summary>
public static class MessageKey
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string SlugExplaniton = nameof(SlugExplaniton);
    public const string InvalidParameterException = nameof(InvalidParameterException);
    public const string UnhandledException = nameof(UnhandledException);
    public const string PostgreBasedException = nameof(PostgreBasedException);
    public const string Unauthorized = nameof(Unauthorized);
    public const string Forbidden = nameof(Forbidden);
    public const string SessionTimeout = "SessionTimeout";
    public const string UserExists = "User.ExistWithSameUsername";
    public const string UserNotFound = "User.NotFound";
    public const string RoleNotFound = "Role.NotFound";
    public const string WrongPassword = "WrongPassword";
    public const string DefaultValueCannotModify = nameof(DefaultValueCannotModify);
    public const string PleaseSendCorrect = nameof(PleaseSendCorrect);
    public const string IdentityInvalidUsername = nameof(IdentityInvalidUsername);
    public const string CannotBeEmpty = nameof(CannotBeEmpty);
    public const string PossibleUIError = "PossibleUIError";
    public const string User = "Global.User";
    public const string Role = "Global.Role";
    public const string Page = "Global.Page";
    public const string PageNotFound = "Page.NotFound";
    public const string PostgreDuplicateDataException = nameof(PostgreDuplicateDataException);
    public const string Hours = nameof(Hours);
    public const string Minutes = nameof(Minutes);
    public const string Seconds = nameof(Seconds);
    public const string Locked = nameof(Locked);
    public const string LockWarning = nameof(LockWarning);
    public const string NamespaceNotFound = nameof(NamespaceNotFound);
    public const string ResourceGroupNotFound = nameof(ResourceGroupNotFound);
    public const string ContentNotFound = nameof(ContentNotFound);
    public const string MediaNotFound = nameof(MediaNotFound);
    public const string Namespace = "Global.Namespace";
    public const string ResourceGroup = "Global.ResourceGroup";
    public const string Content = "Global.Content";
    public const string Media = "Global.Media";
    public const string Language = "Global.Language";
    public const string UnsupportedMediaType = nameof(UnsupportedMediaType);
    public const string OnlyImageFilesAccepted = nameof(OnlyImageFilesAccepted);
    public const string FileSizeMustLowerThan = nameof(FileSizeMustLowerThan);
    public const string Query = nameof(Query);
    public const string QueryType = nameof(QueryType);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
