using System;
using SEIMS.Core;

namespace SEIMS.Services
{
    public class StockNotification
    {
        private readonly FileLogger _fileLogger;

        public StockNotification(FileLogger fileLogger)
        {
            _fileLogger = fileLogger
                ?? throw new ArgumentNullException(nameof(fileLogger));
        }

        public void Subscribe(StockMonitor monitor)
        {
            if (monitor is null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            monitor.OnStockThresholdCritical += DisplayAlert;
        }

        private void DisplayAlert(string message)
        {
            Console.WriteLine(message);
            _fileLogger.WriteLog(message);
        }
    }
}