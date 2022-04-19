using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static TheDangerOfTaskCompletionSource.Tracer;

namespace TheDangerOfTaskCompletionSource
{
    public class DatabaseFacade : IDisposable
    {
        private readonly BlockingCollection<(string item, TaskCompletionSource<string> result)> _queue =
            new BlockingCollection<(string item, TaskCompletionSource<string> result)>();
        private readonly Task _processItemsTask;

        public DatabaseFacade() => _processItemsTask = Task.Run(ProcessItems);

        public void Dispose() => _queue.CompleteAdding();

        public Task SaveAsync(string command)
        {
            var tcs = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _queue.Add((item: command, result: tcs));
            return tcs.Task;
        }

        private async Task ProcessItems()
        {
            Tracer.WriteLine($"DatabaseFacade: running ProcessItems");
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                WriteLine($"DatabaseFacade: executing '{item.item}'...");

                // Waiting a bit to emulate some IO-bound operation
                await Task.Delay(100);
                WriteLine($"DatabaseFacade: Setting the result for '{item.item}'");

                item.result.SetResult("OK");
                WriteLine("DatabaseFacade: DatabaseFacade: done.");
            }
        }
    }

    public class Logger : IDisposable
    {
        private readonly DatabaseFacade _facade;
        private readonly BlockingCollection<string> _queue =
            new BlockingCollection<string>();

        private readonly Task _saveMessageTask;

        public Logger(DatabaseFacade facade) =>
            (_facade, _saveMessageTask) = (facade, Task.Run(SaveMessage));

        public void Dispose() => _queue.CompleteAdding();

        public void WriteLine(string message) => _queue.Add(message);

        private async Task SaveMessage()
        {
            Tracer.WriteLine($"Logger: running SaveMessage");
            foreach (var message in _queue.GetConsumingEnumerable())
            {
                // "Saving" message to the file
                Tracer.WriteLine($"Logger: {message}");
                
                Tracer.WriteLine($"Logger: Sending message to the facade...");

                // And to our database through the facade
                await _facade.SaveAsync(message);
                // When the TaskCompletionSource runs continuation synchronously
                // this line would have the same thread id as the line with "Setting the result for'" printed
                // by DatabaseFacade
                Tracer.WriteLine("Logger: SaveAsync is done. Moving to the next item.");
            }
        }
    }

    public class Tracer
    {
        public static void WriteLine(string message)
        {
            Console.WriteLine($"TID={Thread.CurrentThread.ManagedThreadId}: {message}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            using (var facade = new DatabaseFacade())
            using (var logger = new Logger(facade))
            {
                logger.WriteLine("My message");
                await Task.Delay(100);

                //// Uncomment the Task.Run to unblock the execution in 5 seconds
                //// even for the synchronous continuations of TaskCompletionSource.
                //Task.Run(async () =>
                //{
                //    await Task.Delay(5000);
                //    Tracer.WriteLine("Sending another message to the log!");
                //    // This call will add an item into the logging queue,
                //    // and the call that got stuck on the next item from GetConsumingEnumerable will awake
                //    // and the whole train will be unblocked.
                //    logger.WriteLine("Second Message");
                //});

                await facade.SaveAsync("Another string");
                Console.WriteLine("The string is saved");
            }
        }
    }
}
