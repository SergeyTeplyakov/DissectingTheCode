using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ValueTasks.Internals
{
class StockPrices
{
    private Dictionary<string, decimal> _stockPrices;
    public async Task<decimal> GetStockPriceForAsync(string companyId)
    {
        await InitializeMapIfNeededAsync();
        _stockPrices.TryGetValue(companyId, out var result);
        return result;
    }

    private async Task InitializeMapIfNeededAsync()
    {
        if (_stockPrices == null)
            return;

        await Task.Delay(42);
        // Getting the stock prices from the external source.
        _stockPrices = new Dictionary<string, decimal> { { "MSFT", 42 } };
    }
}
}