using System.Text;

namespace DtC.Episode10;



public static class Program
{
    

    

    public static async Task<string> DoWorkAsync()
    {
        await Task.Delay(1000).ConfigureAwait(false);
        return "Done";
    }

    private static void ProcessData()
    {
        Console.WriteLine("Running ProcessData");
        var result = DoWorkAsync().Result;
        Console.WriteLine(result);
    }

    public static void ProcessDataInParallel()
    {
        var ids = Enumerable.Range(1, 100_000).ToList();
        var syncObject = new object();
        var sb = new StringBuilder();
        Parallel.ForEach(ids,
            id =>
            {
                Append(id);
            });

        void Append(int id)
        {
            lock (syncObject)
            {
                // Suppose the operation is taking 100ms
                Thread.Sleep(100);
                sb.Append(id);
            }
        }
    }

    public static async Task ProcessDataAsync()
    {
        await FetchDataAsync();

        static async Task FetchDataAsync()
        {
            await TryGetDataFromCacheAsync();
        }

        static Task TryGetDataFromCacheAsync()
        {
            var tcs = new TaskCompletionSource();
            return tcs.Task;
        }
    }

    public static void Main(string[] args)
    {
        ProcessDataAsync().GetAwaiter().GetResult();
    }
    
    










    //public static async Task Main(string[] args)
    //{
    //    await ProcessDataAsync();
    //    Console.WriteLine("Done!");
    //    ProcessDataInParallel();
    //    Console.WriteLine("Done Processing Data");
    //    // Set the custom synchronization context
    //    using var syncContext = new SingleThreadSynchronizationContext("MainThread");
    //    SynchronizationContext.SetSynchronizationContext(syncContext);
        
    //    // Using Task.Yield to jump into the custom synchronization context.
    //    await Task.Yield();

    //    ProcessData();
    //    Console.WriteLine("Almost done!");
    //}
}

//public static class TaskExtensions
//{
//    extension(Task)
//    {
//        public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable ForceYielding()
//        {
//            return Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
//        }
//    }

//}