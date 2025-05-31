using System.Diagnostics.Metrics;
// ReSharper disable once EventUnsubscriptionViaAnonymousDelegate


namespace DtC.Episode2.EventHandlers
{

    public class RequestProcessor : IDisposable
    {
        private long UnhandledExcpetionCount = 0;
        private UnhandledExceptionEventHandler? _currentDomainOnUnhandledException;

        public RequestProcessor()
        {
            _currentDomainOnUnhandledException = (_, _) =>
            {
                // Emitting a metric.
                // Using the instance state here.
                Interlocked.Increment(ref UnhandledExcpetionCount);
            };
            AppDomain.CurrentDomain.UnhandledException += _currentDomainOnUnhandledException;
        }

        public void HandleRequest(int data)
        {
            // Handle the request
        }

        public void Dispose()
        {
            // Unsubscribe from the event to prevent memory leaks
            AppDomain.CurrentDomain.UnhandledException -= _currentDomainOnUnhandledException;
        }
    }
}
