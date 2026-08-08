using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace TaskMangment.Tests.Helpers;

public static class TestAsyncQueryable
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source) =>
        new TestAsyncEnumerable<T>(source);
}

internal sealed class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
    {
        _inner = enumerable is IQueryable<T> q ? q : enumerable.AsQueryable();
    }

    public TestAsyncEnumerable(Expression expression)
    {
        _inner = new EnumerableQuery<T>(expression);
    }

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(_inner.GetEnumerator());
}

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) =>
        new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
            .MakeGenericMethod(resultType);

        var result = executeMethod.Invoke(_inner, [expression]);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [result])!;
    }
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
}
