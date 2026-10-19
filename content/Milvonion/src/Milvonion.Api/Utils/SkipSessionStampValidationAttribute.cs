namespace Milvonion.Api.Utils;

/// <summary>
/// Exempts an endpoint from the token revocation check (<c>SecurityStampValidationMiddleware</c>); authentication and
/// authorization still apply. Only for the bootstrap endpoints that must work before the token's user exists: on a
/// fresh database the documentation (Scalar) token names a root user that is not seeded yet, so its stamp cannot be
/// validated. Such an endpoint must be disabled in production and harmless once the database is initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class SkipSessionStampValidationAttribute : Attribute
{
}
