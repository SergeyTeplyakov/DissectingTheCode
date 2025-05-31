using System.Diagnostics.Metrics;

namespace DtC.Episode2.Meters
{
    public class RequestProcessor
    {
        private static readonly Meter _meter = new Meter("MyApp.Core");
        private static readonly Counter<int> _handleCounter = _meter.CreateCounter<int>("HandleRequest.Count");

        public RequestProcessor()
        {
            // _handleCounter = _meter.CreateCounter<int>("HandleRequest.Count");
        }

        public void HandleRequest(int data)
        {
            _handleCounter.Add(1);
        }
    }
}