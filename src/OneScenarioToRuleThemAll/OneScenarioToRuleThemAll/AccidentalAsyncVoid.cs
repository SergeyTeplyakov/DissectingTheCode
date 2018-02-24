using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneScenarioToRuleThemAll
{
    public class File
    {
        public static Task<string> ReadAllTextAsync(string fileName) => null;
        public static Task WriteAllTextAsync(string fileName, string text) => null;
    }
    internal class AccidentalAsyncVoidSample
    {
        private string errorLogFile;

public static async Task<T> ActionWithRetry<T>(Func<Task<T>> provider, Action<Exception> onError)
{
    // Implements a retry logic

    try
            {
                var result = await provider();
                return result;
            }
            catch(Exception e)
            {
                onError(e);
            }

            return default(T);
}
        // Old code
        public Task SaveToDisk(string fileName, byte[] content)
        {
            return Task.Run(async () =>
            {
                using (var file = System.IO.File.OpenRead(fileName))
                {
                    await file.WriteAsync(content, 0, content.Length);
                }
            });
        }

        public Task SaveToDisk2(string fileName, byte[] content)
        {
            return Task.Factory.StartNew(async () =>
            {
                throw null;
                await Awaiters.DetachCurrentSyncContext();
                using (var file = System.IO.File.OpenRead(fileName))
                {
                    await file.WriteAsync(content, 0, content.Length);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public async Task<string> AccidentalAsyncVoid(string fileName)
{
            await Awaiters.DetachCurrentSyncContext();
            return await ActionWithRetry(
        provider:
        () =>
        {
            return File.ReadAllTextAsync(fileName);
        },
        // Can you spot the issue?
        onError:
        async e =>
        {
            await File.WriteAllTextAsync(errorLogFile, e.ToString());
        });
}
public static async Task MayFail(string argument)
{
    if (argument == null)
        throw new ArgumentNullException(nameof(argument));

    await Task.Yield();
}

public static async Task Case1()
{
    try
    {
        MayFail(null).Wait();
    }
    catch (ArgumentException e)
    {
        // Handle the error
    }

    try
    {
        await MayFail(null);
    }
    catch (ArgumentException e)
    {
        // Handle the error
    }
}
        public static async Task OneErrorIsLost()
        {
try
{
    Task<int> task1 = Task.FromException<int>(new ArgumentNullException());

    Task<int> task2 = Task.FromException<int>(new InvalidOperationException());

    // t.Result forces TPL to wrap the exception into AggregateException
    await Task.WhenAll(task1, task2).ContinueWith(t => t.Result);
}
catch(Exception e)
{
    // AggregateException
    Console.WriteLine(e.GetType());
}
        }
    }
}
