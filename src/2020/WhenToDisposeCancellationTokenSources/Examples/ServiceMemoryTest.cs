#nullable enable
using System;
using System.Linq;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WhenToDisposeCancellationTokenSources.Examples
{
public class VeryExpensiveClass
{
    private readonly string[] _s = new string[10_000];
    private int _x;

    public int X
    {
        get
        {
            Console.WriteLine("X");
            return _x;
        }
        set => _x = value;
    }
}
public class Service : IDisposable
{
    private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();

    public async Task LongRunningOperationAsync()
    {
        var veryExpensive = new VeryExpensiveClass();
        await Task.Yield();
        _disposeCts.Token.Register(() => veryExpensive.X++);
    }

    public void Dispose()
    {
        _disposeCts.Cancel();
    }
}

    public class ServiceMemoryTest
    {
        private readonly ITestOutputHelper _output;

        public ServiceMemoryTest(ITestOutputHelper output)
        {
            _output = output;
            DotMemoryUnitTestOutput.SetOutputMethod(data => output.WriteLine(data));
        }

        [Fact]
        public async Task TestMemoryLeak()
        {
            var memoryCheckpoint = dotMemory.Check();
            const int IterationsCount = 1_000;
            using var service = new Service();
            var token = CancellationToken.None;

            for (int i = 0; i < IterationsCount; i++)
            {
                await service.LongRunningOperationAsync(token, disposeRegistration: true);
            }

            dotMemory.Check(memory =>
            {
                _output.WriteLine("Checking");
            });
            //dotMemory.Check(memory =>
            //{
            //    try
            //    {

            //        var count2 = memory.GetObjects(t => t.Type.Is<VeryExpensiveClass>())?.ObjectsCount;
            //        //_output.WriteLine($"Objects: {count2}");
            //        //var count = memory.GetDifference(memoryCheckpoint)
            //        //    .GetSurvivedObjects()
            //        //    .GetObjects(o => o.Type.Is<VeryExpensiveClass>())
            //        //    .ObjectsCount;
            //        //_output.WriteLine($"Objects: {count}");
            //    }
            //    catch (Exception e)
            //    {
            //        _output.WriteLine("ERROR: " + e);
            //    }
            //});


            //for (int i = 0; i < IterationsCount; i++)
            //{
            //    await service.LongRunningOperationAsync(token, disposeRegistration: false);
            //}

            //dotMemory.Check(memory =>
            //{
            //    var count2 = memory.GetObjects(t => t.Type.Is<VeryExpensiveClass>()).ObjectsCount;
            //    _output.WriteLine($"Objects: {count2}");
            //    var count = memory.GetDifference(memoryCheckpoint)
            //        .GetSurvivedObjects()
            //        .GetObjects(o => o.Type.Is<VeryExpensiveClass>())
            //        .ObjectsCount;
            //    _output.WriteLine($"Objects: {count}");
            //});
        }

        //[Fact]
        //public async Task DebugMemoryLeak()
        //{
        //    const int IterationsCount = 1_000;
        //    using var service = new Service();
        //    var token = CancellationToken.None;//new CancellationTokenSource().Token;
        //    for (int i = 0; i < IterationsCount; i++)
        //    {
        //        await service.LongRunningOperationAsync(token);
        //    }
        //}
    }
}