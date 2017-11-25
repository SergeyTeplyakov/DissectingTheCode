using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ValueTasks.Internals
{
    public class StockPrices
    {
        private Dictionary<string, decimal> _stocks;

public async Task<decimal> GetStockPriceForAsync(string companyId)
{
    if (string.IsNullOrEmpty(companyId)) throw new ArgumentNullException();
    
            
            AsyncLocal<int> al = new AsyncLocal<int>();
    al.Value = 42;
    await InitializeLocalStoreIfNeededAsync();

    Console.WriteLine("Async local: " + al.Value);
    _stocks.TryGetValue(companyId, out var result);
    return result;

    //result = await GetQuoteFromTheRemoteAsync(companyId);
    //_stocks[companyId] = result;
    //return result;
}

        private async Task InitializeLocalStoreIfNeededAsync()
        {
            // Reeds the quotes from disk.
            if (_stocks == null)
            {
                await Task.Delay(42);
                _stocks = new Dictionary<string, decimal> {{"MSFT", 42}};
            }
        }
    }
}