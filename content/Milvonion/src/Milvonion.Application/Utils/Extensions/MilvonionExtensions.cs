using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Abstract;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Milvonion.Application.Utils.Extensions;

/// <summary>
/// Contains extension methods for the Milvonion.
/// </summary>
public static partial class MilvonionExtensions
{
    /// <summary>
    /// Gets token from header via HttpContextAccessor.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static string GetTokenFromHeader(this IHttpContextAccessor httpContextAccessor)
         => httpContextAccessor.HttpContext.GetTokenFromHeader();

    /// <summary>
    /// Gets token from header via HttpContext.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string GetTokenFromHeader(this HttpContext httpContext)
         => httpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").LastOrDefault();

    /// <summary>
    /// Checks whether the current user is the user whose name is given as a parameter via HttpContext.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public static bool IsCurrentUser(this IHttpContextAccessor httpContextAccessor, string userName)
        => httpContextAccessor.HttpContext.User.Identity.Name == userName;

    /// <summary>
    /// Gets current user name via HttpContextAccessor.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string CurrentUserName(this HttpContext httpContext) => httpContext.User.Identity.Name;

    /// <summary>
    /// Get current user type from claims.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static UserType? GetCurrentUserType(this HttpContext httpContext)
    {
        var claims = httpContext.GetCurrentUserClaims();

        var userType = claims.FirstOrDefault(p => p.Type == GlobalConstant.UserTypeClaimName);

        return userType != null ? Enum.Parse<UserType>(userType.Value) : null;
    }

    /// <summary>
    /// Get current user permissions from claims.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetCurrentUserPermissions(this HttpContext httpContext)
    {
        var claims = httpContext.GetCurrentUserClaims();

        var permissions = claims.Where(p => p.Type == ClaimTypes.Role).Select(i => i.Value);

        return permissions;
    }

    /// <summary>
    /// Checks whether the current user is the user whose name is given as a parameter via HttpContext.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static IEnumerable<Claim> GetCurrentUserClaims(this HttpContext httpContext)
    {
        if (!httpContext.User.Identity.IsAuthenticated)
            return [];

        var token = httpContext.GetTokenFromHeader();

        var tokenManager = httpContext.RequestServices.GetService<IMilvaTokenManager>();

        var claimsPrincipal = tokenManager.GetClaimsPrincipalIfValid(token);

        return claimsPrincipal?.Claims ?? [];
    }

    /// <summary>
    /// Converts the input string to lowercase and removes diacritics (accents) from characters.
    /// </summary>
    /// <param name="input">The input string to be converted.</param>
    /// <returns>The converted string with lowercase letters and no diacritics.</returns>
    public static string ToLowerAndNonSpacingUnicode(this string input)
    {
        // Remove diacritics (accents) from characters
        string normalizedString = input.Normalize(NormalizationForm.FormD);
        char[] charArray = normalizedString.ToCharArray();
        var stringWithoutDiacriticsBuilder = new StringBuilder();

        foreach (char c in charArray)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringWithoutDiacriticsBuilder.Append(c);
            }
        }

        // Remove spaces and convert to lowercase
        string result = ReplaceAndToLowerRegex().Replace(stringWithoutDiacriticsBuilder.ToString(), "").ToLower();

        return result;
    }

    /// <summary>
    /// Calculates base 64 string length and returns whether it is valid.
    /// </summary>
    /// <param name="base64String"></param>
    /// <returns></returns>
    public static bool IsBase64StringValidLength(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return true;

        int base64StringLength = base64String.Length;

        int paddingCount;

        if (base64String.EndsWith('='))
        {
            paddingCount = base64String.EndsWith("==") ? 2 : 1;
        }
        else
        {
            paddingCount = base64String.EndsWith("==") ? 2 : 0;
        }

        long originalSize = (base64StringLength * 3 / 4) - paddingCount;

        double megabytes = (double)originalSize / (1024 * 1024);

        if (megabytes > 1)
            return false;

        return true;
    }

    /// <summary>
    /// Validates the base64 string by checking the file extension.
    /// </summary>
    /// <param name="base64String"></param>
    /// <returns></returns>
    public static bool IsBase64StringHasValidFileExtension(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return true;

        if (base64String.Contains("data:"))
        {
            var dataSection = base64String[..base64String.IndexOf(";base64")];

            if (dataSection.Contains(MimeTypeNames.ImageJpeg))
                return true;
            else if (dataSection.Contains(MimeTypeNames.ImagePng))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given string is a valid data uri base64 format.
    /// </summary>
    /// <param name="dataUri"></param>
    /// <returns></returns>
    public static bool IsValidDataUri(string dataUri) => DataUriRegex().IsMatch(dataUri);

    /// <summary>
    /// Converts data uri base64 string to plain text base64 string.
    /// </summary>
    /// <param name="dataUriBase64"></param>
    /// <returns></returns>
    public static byte[] DataUriToPlainText(string dataUriBase64)
    {
        if (string.IsNullOrEmpty(dataUriBase64))
            return [];

        var base64String = dataUriBase64.Split(";base64,")[1];

        var array = Convert.FromBase64String(base64String);

        return array;
    }

    [GeneratedRegex(@"^data:(?<mediatype>[\w/+.-]+);base64,(?<data>[a-zA-Z0-9+/]+={0,2})$")]
    private static partial Regex DataUriRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex ReplaceAndToLowerRegex();
}
