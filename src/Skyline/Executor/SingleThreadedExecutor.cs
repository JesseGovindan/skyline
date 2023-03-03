using Microsoft.VisualStudio.Threading;

class SingleThreadedExecutor : IExecutor
{
    private SingleThreadedSynchronizationContext context;
    private SingleThreadedSynchronizationContext.Frame frame;

    public SingleThreadedExecutor()
    {
        context = new SingleThreadedSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);
        frame = new SingleThreadedSynchronizationContext.Frame();
    }

    public void Dispose()
    {
        frame.Continue = false;
    }

    public void Run(ActAsync<IDisposable> action)
    {
        action(this).ContinueWith(task => {
            if (task.IsFaulted) {
                Dispose();
            }
        });
        context.PushFrame(frame);
    }
}