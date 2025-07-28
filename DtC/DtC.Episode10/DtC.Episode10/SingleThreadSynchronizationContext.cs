using System.Collections.Concurrent;

namespace DtC.Episode10;

/// <summary>
/// A custom SynchronizationContext that executes all operations on a dedicated single thread.
/// </summary>
public class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<WorkItem> _workQueue = new();
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private volatile bool _isDisposed;
    private readonly string _threadName;

    public SingleThreadSynchronizationContext(string threadName = "SingleThreadSyncContext")
    {
        _threadName = threadName;
        
        _thread = new Thread(ThreadProc)
        {
            Name = _threadName,
            IsBackground = false
        };
        
        _thread.Start();
    }

    /// <summary>
    /// Gets the ID of the dedicated thread.
    /// </summary>
    public int ThreadId => _thread.ManagedThreadId;

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        _workQueue.Add(new WorkItem(d, state, false));
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        // If we're already on the target thread, execute immediately to avoid deadlock
        if (Thread.CurrentThread.ManagedThreadId == _thread.ManagedThreadId)
        {
            d(state);
            return;
        }

        var workItem = new WorkItem(d, state, true);
        _workQueue.Add(workItem);
        
        // Wait for completion
        workItem.CompletionEvent.Wait();
        
        // Re-throw any exception that occurred
        if (workItem.Exception != null)
            throw workItem.Exception;
    }

    /// <inheritdoc />
    public override SynchronizationContext CreateCopy()
    {
        return new SingleThreadSynchronizationContext(_threadName + "_Copy");
    }

    /// <summary>
    /// The main thread procedure that processes work items.
    /// </summary>
    private void ThreadProc()
    {
        // Set this context as the synchronization context for the thread
        SynchronizationContext.SetSynchronizationContext(this);
        
        Console.WriteLine($"SingleThreadSynchronizationContext started on thread {Thread.CurrentThread.ManagedThreadId}");

        try
        {
            foreach (var workItem in _workQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                Console.WriteLine($"Processing work item on thread {Thread.CurrentThread.ManagedThreadId}");
                try
                {
                    Console.WriteLine("Before executing work item callback");
                    workItem.Callback(workItem.State);
                    Console.WriteLine("After executing work item callback");
                }
                catch (Exception ex)
                {
                    if (workItem.IsSynchronous)
                    {
                        workItem.Exception = ex;
                    }
                    else
                    {
                        // For async operations, we can't throw back to the caller
                        // Log or handle the exception as appropriate for your application
                        Console.WriteLine($"Unhandled exception in async operation: {ex}");
                    }
                }
                finally
                {
                    if (workItem.IsSynchronous)
                    {
                        workItem.CompletionEvent.Set();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the cancellation token is cancelled
            Console.WriteLine("SingleThreadSynchronizationContext thread was cancelled");
        }
        catch (InvalidOperationException)
        {
            // This is expected when the collection is marked as complete for adding
        }
        
        Console.WriteLine($"SingleThreadSynchronizationContext stopped on thread {Thread.CurrentThread.ManagedThreadId}");
    }

    /// <summary>
    /// Disposes the synchronization context and shuts down the dedicated thread.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Console.WriteLine($"Disposing SingleThreadSynchronizationContext on thread {Thread.CurrentThread.ManagedThreadId}");
        
        // Cancel the thread operation and complete adding to trigger graceful shutdown
        Console.WriteLine("Cancelling cancellation token and completing work queue.");
        _cancellationTokenSource.Cancel();
        _workQueue.CompleteAdding();
        
        
        if (!_thread.Join(TimeSpan.FromSeconds(5)))
        {
            Console.WriteLine("Warning: Thread did not shut down gracefully within 5 seconds");
        }
        
        _workQueue.Dispose();
        _cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Represents a work item to be executed on the dedicated thread.
    /// </summary>
    private class WorkItem
    {
        public SendOrPostCallback Callback { get; }
        public object? State { get; }
        public bool IsSynchronous { get; }
        public ManualResetEventSlim CompletionEvent { get; }
        public Exception? Exception { get; set; }

        public WorkItem(SendOrPostCallback callback, object? state, bool isSynchronous)
        {
            Callback = callback;
            State = state;
            IsSynchronous = isSynchronous;
            CompletionEvent = isSynchronous ? new ManualResetEventSlim(false) : null!;
        }
    }
}