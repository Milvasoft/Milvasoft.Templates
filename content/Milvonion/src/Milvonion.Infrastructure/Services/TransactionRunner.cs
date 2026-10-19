using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Ef.Transaction;
using Milvonion.Application.Interfaces;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Default <see cref="ITransactionRunner"/>. The <see cref="TransactionAttribute"/> on <see cref="RunAsync"/>
/// makes the transaction interceptor open a transaction on the scoped <c>MilvonionDbContext</c> before the delegate
/// runs and commit it afterwards (rolling back on exception). It is the same scoped context the repositories
/// resolve, so the writes inside the delegate enlist in that transaction.
/// <para>
/// Being resolved and called through the <see cref="ITransactionRunner"/> interface is what lets the proxy
/// intercept the call. A handler cannot get the same effect from a local <c>[Transaction]</c> method: a
/// same-instance call bypasses the proxy (self-invocation), so the interceptor would never run.
/// </para>
/// </summary>
public class TransactionRunner : IInterceptable, ITransactionRunner
{
    /// <inheritdoc/>
    // Authored async (await work()) rather than expression-bodied `=> work()`: the transaction interceptor
    // must see this method's task complete only after the delegate finishes, so its commit runs strictly
    // after the delegate's writes. Returning the delegate's incomplete task directly lets the commit race the
    // delegate on the shared DbContext ("a second operation was started on this context").
    [Transaction]
    public async Task RunAsync(Func<Task> work) => await work();
}
