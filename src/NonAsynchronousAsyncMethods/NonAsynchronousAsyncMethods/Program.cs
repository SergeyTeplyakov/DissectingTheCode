using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NonAsynchronousAsyncMethods
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Task task = Task.Run(async () =>
            {
                Console.WriteLine($"{DateTime.UtcNow}: Running a core task.");
                await Task.Delay(1000);
                Console.WriteLine($"{DateTime.UtcNow}: a core task is about to finish.");
            });

            await Task.WhenAll(
                ContinueAfter(task, 1),
                ContinueAfter(task, 2),
                ContinueAfter(task, 3)
                );
        }

        static async Task ContinueAfter(Task task, int idx)
        {
            //await Task.Yield();
            await task;
            Console.WriteLine($"{DateTime.UtcNow}: {idx} - starting long running operation.");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine($"{DateTime.UtcNow}: {idx} - long running operation is done.");
        }

        //static Task ContinueAfter(Task task, int idx)
        //{
        //    return task.ContinueWith(
        //        _ =>
        //        {
        //            Console.WriteLine($"{DateTime.UtcNow}: {idx} - starting long running operation.");
        //            Thread.Sleep(TimeSpan.FromSeconds(2));
        //            Console.WriteLine($"{DateTime.UtcNow}: {idx} - long running operation is done.");
        //        });
        //}
    }
}
