using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneScenarioToRuleThemAll
{
    class ExceptionHandlingSamples
    {
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
