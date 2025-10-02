//namespace DtC.Episode10;


//public static class Program
//{
//    public static async Task ProcessDataAsync()
//    {
//        WriteLine("Start");
//        string result = await GetDataAsync();
//        WriteLine(result);
//    }

//    private static Task<string> GetDataAsync()
//    {
//        return Task.Run(() =>
//        {
//            WriteLine("Getting data...");
//            return "42";
//        });
//    }

//    public static async Task Main(string[] args)
//    {
//        // Set the custom synchronization context
//        var syncContext = new SingleThreadSynchronizationContext("MainThread");
//        SynchronizationContext.SetSynchronizationContext(syncContext);

//        // Using Task.Yield to jump into the custom synchronization context.
//        await Task.Yield();

//        await ProcessDataAsync();
//    }

//    static void WriteLine(string message)
//    {
//        Console.WriteLine($"[{DateTime.Now}] [{Environment.CurrentManagedThreadId}] {message}");
//    }
//}