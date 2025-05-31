using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DtC.Episode2
{
    internal class DataGenerator : IDisposable
    {
        private const int DataSizeBytes = 50 * 1024 * 1024; // 50 MB
        private const int MaxResults = 100;
        private readonly List<byte[]> _results = new();
        private int _currentIndex = 0;
        private readonly Timer _timer;
        private readonly object _lock = new();

        public DataGenerator()
        {
            _timer = new Timer(state => GenerateData(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void GenerateData()
        {
            var data = new byte[DataSizeBytes];
            Random.Shared.NextBytes(data);

            lock (_lock)
            {
                if (_results.Count < MaxResults)
                {
                    _results.Add(data);
                }
                else
                {
                    _results[_currentIndex] = data;
                    _currentIndex = (_currentIndex + 1) % MaxResults;
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}