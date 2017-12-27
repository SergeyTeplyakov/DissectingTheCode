using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ValueTasks.Internals.StateMachineByHand
{
    public class StockPrices
    {
class GetStockPriceForAsync_StateMachine
{
    enum State { Start, Step1, }
    private readonly StockPrices @this;
    private readonly string _companyId;
    private readonly TaskCompletionSource<decimal> _tcs;
    private Task _initializeMapIfNeededTask;
    private State _state = State.Start;

    public GetStockPriceForAsync_StateMachine(StockPrices @this, string companyId)
    {
        this.@this = @this;
        _companyId = companyId;
    }

    public void Start()
    {
        try
        {
            if (_state == State.Start)
            {
// Step 1 of the generated state machine:

if (string.IsNullOrEmpty(_companyId)) throw new ArgumentNullException();
_initializeMapIfNeededTask = @this.InitializeMapIfNeeded();


                        // Schedule continuation
_state = State.Step1;
_initializeMapIfNeededTask.ContinueWith(_ => Start());
                    }
            else if (_state == State.Step1)
            {
                // Need to check the error and the cancel case first
                if (_initializeMapIfNeededTask.Status == TaskStatus.Canceled)
                    _tcs.SetCanceled();
                else if (_initializeMapIfNeededTask.Status == TaskStatus.Faulted)
                    _tcs.SetException(_initializeMapIfNeededTask.Exception.InnerException);
                else
                {
// The code between first await and the rest of the method

@this._store.TryGetValue(_companyId, out var result);
_tcs.SetResult(result); // The caller gets the result back
                        }
            }
        }
        catch (Exception e)
        {
            _tcs.SetException(e);
        }
    }

    public Task<decimal> Task => _tcs.Task;
}

public Task<decimal> GetStockPriceForAsync(string companyId)
{
    var stateMachine = new GetStockPriceForAsync_StateMachine(this, companyId);
    stateMachine.Start();
    return stateMachine.Task;
}

        public async Task<decimal> GetStockPriceForAsyncOrigin(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                throw new ArgumentNullException();
            }

            await InitializeLocalStoreIfNeededAsync();

            if (_store.TryGetValue(companyId, out var result))
            {
                return result;
            }

            result = await GetQuoteFromTheRemoteAsync(companyId);
            _store[companyId] = result;
            return result;
        }

        private Dictionary<string, decimal> _store;
        private async Task InitializeLocalStoreIfNeededAsync()
        {
            // Reeds the quotes from disk.
            if (_store == null)
            {
                await Task.Delay(42);
                _store = new Dictionary<string, decimal>()
                {
                    ["MSFT"] = 42
                };
            }
        }

        private Task<decimal> GetQuoteFromTheRemoteAsync(string companyId)
        {
            return Task.FromResult<decimal>(42);
        }
        /*
         * await InitializeMapIfNeeded();
            return _stockPrices[companyId];
         * */
        private Task InitializeMapIfNeeded() => null;

        
        //{
        //    _GetStockPriceForAsync_d__1 _GetStockPriceForAsync_d__;
        //    _GetStockPriceForAsync_d__.__4__this = this;
        //    _GetStockPriceForAsync_d__.companyId = companyId;
        //    _GetStockPriceForAsync_d__.__t__builder = AsyncTaskMethodBuilder<decimal>.Create();
        //    _GetStockPriceForAsync_d__.__1__state = -1;
        //    AsyncTaskMethodBuilder<decimal> __t__builder = _GetStockPriceForAsync_d__.__t__builder;
        //    __t__builder.Start<_GetStockPriceForAsync_d__1>(ref _GetStockPriceForAsync_d__);
        //    return _GetStockPriceForAsync_d__.__t__builder.Task;
        //}

    }
}