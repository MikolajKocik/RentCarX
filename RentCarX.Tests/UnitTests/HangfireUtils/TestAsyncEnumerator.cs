namespace RentCarX.Tests.UnitTests.HangfireUtils;

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;
    public T Current => _enumerator.Current;
    public TestAsyncEnumerator(IEnumerator<T> enumerator) => _enumerator = enumerator;
    public ValueTask<bool> MoveNextAsync() => new(_enumerator.MoveNext());
    public ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
        return new ValueTask();
    }
}
