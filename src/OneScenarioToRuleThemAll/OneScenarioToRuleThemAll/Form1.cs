using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneScenarioToRuleThemAll
{
    internal class StockPrices
    {
        public string GetStockPricesFor(string symbol)
        {
            return "42";
        }

        public Task<string> GetStockPricesForAsync(string symbol)
        {
            throw new NotImplementedException();
        }
    }

    public partial class Form1 : Form
    {
        private readonly StockPrices _stockPrices = new StockPrices();
        public Form1()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Application.ThreadException += Application_ThreadException;
            InitializeComponent();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Action lambda = () =>
            {
                textBox.Text = "U " + e.ExceptionObject.ToString();
            };

            this.Invoke(lambda);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Action lambda = () =>
            {
                textBox.Text = "Tsk " + e.Exception.ToString();
                e.SetObserved();
            };

            this.Invoke(lambda);
        }

// Sync
private void buttonOk_Click(object sender, EventArgs args)
{
    textBox.Text = "Running2..";
            //new AccidentalAsyncVoidSample().AccidentalAsyncVoid(null).GetAwaiter().GetResult();
            new AccidentalAsyncVoidSample().SaveToDisk2("unknown.txt", new byte[] { 42 }).GetAwaiter().GetResult();
            var result = _stockPrices.GetStockPricesFor("MSFT");
            textBox.Text = "Result 2 is: " + result;
}

// Async
private async void buttonOk_ClickAsync(object sender, EventArgs args)
{
    textBox.Text = "Running..";
    var result = await _stockPrices.GetStockPricesForAsync("MSFT");
    textBox.Text = "Result is: " + result;
}


        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            textBox.Text = e.Exception.ToString();
        }

// Inside AwesomeStockLib.dll
private async Task<decimal> GetStockPricesForAsync(string symbol)
{
            await Awaiters.DetachCurrentSyncContext();
            await Task.Yield();
            if (symbol == "MSFT")
                throw new ArgumentNullException(nameof(symbol));
            // We know that the initialization step is very fast,
            // and synchronous in most cases,
            // let's wait for the result synchronously to for "performance reasons".
            //InitializeIfNeededAsync().Wait();
            //return Task.FromResult((decimal)42);
            return 42;
}

// Inside AwesomeStockLib.dll
//private Task InitializeIfNeededAsync() => Task.Delay(1);
public async Task InitializeIfNeededAsync()
{
    // Detach the current sync context from the async invocation chain.
    await Awaiters.DetachCurrentSyncContext();
    await cache.InitializeAsync();
    await Task.Delay(1);
}

        private readonly Cache cache = new Cache();
    }

    class Cache
    {
        public async Task InitializeAsync() => await Task.FromResult(42);
    }
}
