namespace Milvonion.Application.Utils.Extensions;

/// <summary>
/// Contains helpers for the paging values of list requests.
/// </summary>
public static class PagingExtensions
{
    /// <summary>
    /// The page size a list handler serves: the client's <paramref name="rowCount"/>, or <paramref name="defaultRowCount"/>
    /// when it sent none, clamped to <see cref="GlobalConstant.MinRowCount"/>..<paramref name="maxRowCount"/> so one
    /// request cannot ask for an unbounded page.
    /// </summary>
    /// <param name="rowCount"></param>
    /// <param name="defaultRowCount"></param>
    /// <param name="maxRowCount">Upper bound; <see cref="GlobalConstant.MaxRowCount"/> unless the list also backs an export.</param>
    /// <returns></returns>
    public static int ClampRowCount(this int? rowCount, int defaultRowCount, int maxRowCount = GlobalConstant.MaxRowCount)
        => Math.Clamp(rowCount ?? defaultRowCount, GlobalConstant.MinRowCount, maxRowCount);
}
