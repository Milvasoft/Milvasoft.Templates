using Microsoft.Extensions.DependencyInjection;

namespace Milvonion.Infrastructure.LazyImpl;

/// <summary>
/// Custom <see cref="Lazy{T}"/> class.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Constructor of <see cref="MilvonionLazy{T}"/>.
/// </remarks>
/// <param name="serviceProvider"></param>
public class MilvonionLazy<T>(IServiceProvider serviceProvider) : Lazy<T>(serviceProvider.GetRequiredService<T>)
{
}
