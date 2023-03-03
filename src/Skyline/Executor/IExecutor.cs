delegate Task ActAsync<T>(object T);

interface IExecutor : IDisposable {
    void Run(ActAsync<IDisposable> action);
}