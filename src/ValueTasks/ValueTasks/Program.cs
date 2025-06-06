﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using ObjectLayoutInspector;
using ValueTasks.CustomAwaiter;
using ValueTasks.Internals;

namespace ValueTasks
{
    class CustomSynchronizationContext : SynchronizationContext
    {
        
    }

    class Program
    {
        private static void PrintContext(string tag)
        {
            Console.WriteLine($"[{tag}] Sync context is '{(SynchronizationContext.Current is CustomSynchronizationContext ? "Custom" : "None")}'.");
        }

        private static async Task WithSyncContext()
        {
            PrintContext("Start");

            await Task.Run(() => Thread.Sleep(42)).ConfigureAwait(true);

            PrintContext("AfterYield");

            await Task.Run(async () =>
            {
                PrintContext("InsideRun");

                await Task.Yield();
                PrintContext("InsideRunAfterAwait");
                return 422;
            });

            PrintContext("AfterAwait");
        }

        static async Task ExecutionContextInAsyncMethod()
        {
            var li = new AsyncLocal<int>();
            li.Value = 42;
            await Task.Delay(42);

            // The context is implicitely captured. li.Value is 42
            Console.WriteLine("After first await: " + li.Value);

            var tsk2 = Task.Yield();
            tsk2.GetAwaiter().UnsafeOnCompleted(() =>
            {
                // The context is not captured: li.Value is 0
                Console.WriteLine("Inside UnsafeOnCompleted: " + li.Value);
            });

            await tsk2;

            // The context is captured: li.Value is 42
            Console.WriteLine("After second await: " + li.Value);
        }
        static Task ExecutionContextInAction()
{
    var li = new AsyncLocal<int>();
    li.Value = 42;

    return Task.Run(() =>
    {
        // Task.Run restores the execution context
        Console.WriteLine("In Task.Run: " + li.Value);
    }).ContinueWith(_ =>
    {
        // The continuation restores the execution context as well
        Console.WriteLine("In Task.ContinueWith: " + li.Value);
    });
}

        static void Main(string[] args)
        {
            //Console.WriteLine("Main: before Example.Foo");
            //var tsk = Example.Foo();

            //Console.WriteLine("Main: before tsk.GetAwaiter().GetResult()");
            //tsk.GetAwaiter().GetResult();
            //BenchmarkRunner.Run<Benchmarks>();

            SynchronizationContext.SetSynchronizationContext(new CustomSynchronizationContext());
            WithSyncContext().GetAwaiter().GetResult();

            //ThreadPool.SetMinThreads(42, 42);
            //ExecutionContextInAsyncMethod().GetAwaiter().GetResult();

            //var sp = new StockPrices();
            //var p = sp.GetStockPriceForAsync("MSFT").GetAwaiter().GetResult();
            //Console.WriteLine(p);


            //var types = Assembly.GetCallingAssembly().GetTypes().Where(t => t.FullName.Contains("StockPrices")).ToList();
            //Console.WriteLine($"Found {types.Count} types...");
            //foreach (var t in types)
            //{
            //    var layout = TypeLayout.GetLayout(t);
            //    Console.WriteLine(layout.ToString(true));
            //}

            //Console.WriteLine(TypeLayout.GetLayout<TaskAwaiter>());

            Console.ReadLine();
        }
    }
}
