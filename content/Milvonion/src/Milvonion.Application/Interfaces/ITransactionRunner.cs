namespace Milvonion.Application.Interfaces;

/// <summary>
/// Runs a unit of database work inside a real transaction that the transaction interceptor opens on the
/// (proxied) implementation. Handlers use this to keep ONLY their DB mutations transactional while doing
/// external work (cache writes, notifications, e-mail) AFTER the commit, instead of wrapping the
/// whole handler in <c>[Transaction]</c> (which would drag those external calls inside the transaction).
/// <para>
/// Milvasoft interception is proxy-based, so the transaction only fires when the method is invoked through
/// this injected interface. A private <c>[Transaction]</c> method on the handler itself would NOT open a
/// transaction: a same-instance call bypasses the proxy (self-invocation), so the interceptor never runs.
/// </para>
/// </summary>
public interface ITransactionRunner
{
    /// <summary>
    /// Runs <paramref name="work"/> inside a transaction: commits when it completes, rolls back if it throws.
    /// Only put database mutations in <paramref name="work"/>; do external calls after this returns.
    /// </summary>
    /// <param name="work">The database work to run transactionally.</param>
    Task RunAsync(Func<Task> work);
}
