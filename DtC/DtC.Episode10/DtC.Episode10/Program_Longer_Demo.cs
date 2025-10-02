//namespace DtC.Episode10;



//public static class Program
//{

//    private static async Task GetResultAsync()
//    {
//        WriteLine("Inside GetResultAsync, about to await...");
//        // When we await, the method returns an incomplete Task.
//        // The continuation is scheduled to run on the SynchronizationContext.
//        await Task.Delay(500);
//        WriteLine("This line will never be reached.");
//    }

//    public static async Task AsyncOperation()
//    {
//        Console.WriteLine("AsyncOperation started.");
//        await Task.Delay(100);

//        GetResultAsync().GetAwaiter().GetResult();
//        Console.WriteLine("AsyncOperation ended.");
//    }

//    public static async Task Main(string[] args)
//    {
//        // Set the custom synchronization context
//        using var syncContext = new SingleThreadSynchronizationContext("MainThread");
//        SynchronizationContext.SetSynchronizationContext(syncContext);

//        // Using Task.Yield to jump into the custom synchronization context.
//        await Task.Yield();
//        //await CauseDeadlock();
//        //Console.WriteLine("Deadlock avoided, continuing...");
//        await AsyncOperation();
//        return;

//        WriteLine("About to run TopLevelOperation.");
//        await ProcessDataAndUpdateUI();

//        // We need to jump from the synchronization context in order to dispose it properly.
//        // Because this line is executed by the thread, so the thread.
//        // Resetting the sync context and then yielding to the thread pool.
//        SynchronizationContext.SetSynchronizationContext(null);
//        await Task.Yield();
//    }

//    // Application code:
//    // Each step should be executed in sync context!
//    static async Task ProcessDataAndUpdateUI()
//    {
//        WriteLine("Starting top-level operation..."); // dedicated thread

//        // This is the application code, so don't use ConfigureAwait here
//        await ProcessDataAsync(42);

//        WriteLine("Top-level operation completed."); // dedicated thread
//    }

//    // Library code:
//    // Every step (except the first one) should NOT be executed
//    // in sync context!
//    static async Task<string> ProcessDataAsync(int id)
//    {
//        WriteLine("Starting data processing...");
//        var data = await FetchDataAsync(id).ConfigureAwait(false);
//        // this is an expensive operation that can take a lot of resources!
//        WriteLine($"Data processed: {data}");

//        await SaveDataAsync(id, data).ConfigureAwait(false);
//        WriteLine("Data saved successfully.");
//        return data;
//    }

//    static async Task<string> FetchDataAsync(int id)
//    {
//        await Task.Delay(500); // Simulate async work
//        return $"Data for ID {id}";
//    }

//    static async Task SaveDataAsync(int id, string data)
//    {
//        await Task.Delay(500); // Simulate async work
//    }

//    static void WriteLine(string message)
//    {
//        Console.WriteLine($"[{DateTime.Now}] [{Environment.CurrentManagedThreadId}] {message}");
//    }
//}

////public static class TaskExtensions
////{
////    extension(Task)
////    {
////        public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable ForceYielding()
////        {
////            return Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
////        }
////    }

////}