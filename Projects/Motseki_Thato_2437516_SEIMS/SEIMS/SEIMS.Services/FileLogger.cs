using System;
using System.IO;
using System.Text;

namespace SEIMS.Services
{
    public class FileLogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A log file path is required.",
                    nameof(filePath));
            }

            _filePath = filePath;
        }

        public void WriteLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "The log message cannot be empty.",
                    nameof(message));
            }

            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string logEntry =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

            using FileStream fileStream = new FileStream(
                _filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read);

            using StreamWriter writer = new StreamWriter(
                fileStream,
                Encoding.UTF8);

            writer.WriteLine(logEntry);
        }

        public string ReadLogs()
        {
            if (!File.Exists(_filePath))
            {
                return string.Empty;
            }

            using FileStream fileStream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            using StreamReader reader = new StreamReader(
                fileStream,
                Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}