using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ValueTasks.Internals.Decompiled
{
    public class StockPrices
    {
struct _GetStockPriceFor_d__1 : IAsyncStateMachine
{
    public StockPrices __this;
    public string companyId;
    public AsyncTaskMethodBuilder<decimal> __builder;
    public int __state;
    private TaskAwaiter __task1Awaiter;

    public void MoveNext()
    {
        decimal result;
        try
        {
            TaskAwaiter awaiter;
            if (__state != 0)
            {
                // State 1 of the generated state machine:
                if (string.IsNullOrEmpty(companyId)) throw new ArgumentNullException();

                awaiter = __this.InitializeLocalStoreIfNeededAsync().GetAwaiter();
                        
                // Hot path optimization: if the task is completed,
                // the state machine automatically moves to the next step
                if (!awaiter.IsCompleted)
                {
                    __state = 0;
                    __task1Awaiter = awaiter;
                            
                    // The following call will eventually cause boxing of the state machine.
                    __builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    return;
                }
            }
            else
            {
                awaiter = __task1Awaiter;
                __task1Awaiter = default(TaskAwaiter);
                __state = -1;
            }

            // GetResult returns void, but it'll throw if the awaited task failed.
            // This excception is catched later and changes the resulting task.
            awaiter.GetResult();
            __this._stocks.TryGetValue(companyId, out result);
        }
        catch (Exception exception)
        {
            // Final state: failure
            __state = -2;
            __builder.SetException(exception);
            return;
        }

        // Final state: success
        __state = -2;
        __builder.SetResult(result);
    }

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
    {
        __builder.SetStateMachine(stateMachine);
    }
}

        private Dictionary<string, decimal> _stocks;
        public async Task InitializeLocalStoreIfNeededAsync()
        {
        }

        [AsyncStateMachine(typeof(_GetStockPriceFor_d__1))]
public Task<decimal> GetStockPriceFor(string companyId)
{
    _GetStockPriceFor_d__1 _GetStockPriceFor_d__;
    _GetStockPriceFor_d__.__this = this;
    _GetStockPriceFor_d__.companyId = companyId;
    _GetStockPriceFor_d__.__builder = AsyncTaskMethodBuilder<decimal>.Create();
    _GetStockPriceFor_d__.__state = -1;
    var __t__builder = _GetStockPriceFor_d__.__builder;
    __t__builder.Start<_GetStockPriceFor_d__1>(ref _GetStockPriceFor_d__);
    return _GetStockPriceFor_d__.__builder.Task;
}
    }
}