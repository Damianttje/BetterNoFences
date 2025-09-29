using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoFences.Util
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public class Logger : IDisposable
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _logWriterTask;
        private readonly string _logFilePath;
        private bool _disposed = false;

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
        public bool EnableFileOutput { get; set; } = true;

        private Logger()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fenceless");
            Directory.CreateDirectory(appDataPath);
            _logFilePath = Path.Combine(appDataPath, "application.log");

            // Start background log writer task
            _logWriterTask = Task.Run(ProcessLogQueue, _cancellationTokenSource.Token);

            // Log startup
            Info("Logger initialized", "Logger");
        }

        public void Debug(string message, string category = null)
        {
            Log(LogLevel.Debug, message, category);
        }

        public void Info(string message, string category = null)
        {
            Log(LogLevel.Info, message, category);
        }

        public void Warning(string message, string category = null)
        {
            Log(LogLevel.Warning, message, category);
        }

        public void Error(string message, string category = null, Exception exception = null)
        {
            var fullMessage = exception != null ? $"{message}\nException: {exception}" : message;
            Log(LogLevel.Error, fullMessage, category);
        }

        public void Critical(string message, string category = null, Exception exception = null)
        {
            var fullMessage = exception != null ? $"{message}\nException: {exception}" : message;
            Log(LogLevel.Critical, fullMessage, category);
        }

        private void Log(LogLevel level, string message, string category)
        {
            if (level < MinimumLogLevel) return;

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Category = category ?? "General",
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            _logQueue.Enqueue(logEntry);
        }

        private async Task ProcessLogQueue()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (_logQueue.TryDequeue(out var logEntry))
                    {
                        await WriteLogEntry(logEntry);
                    }
                    else
                    {
                        await Task.Delay(50, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    MessageBox.Show("An fatal error occurred in the logging system. Logging will be disabled.", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Process remaining log entries
            while (_logQueue.TryDequeue(out var logEntry))
            {
                try
                {
                    await WriteLogEntry(logEntry);
                }
                catch
                {
                    // Ignore errors during shutdown
                }
            }
        }

        private async Task WriteLogEntry(LogEntry logEntry)
        {
            var formattedMessage = FormatLogEntry(logEntry);

            // Write to file (primary and only logging destination)
            if (EnableFileOutput)
            {
                try
                {
                    await File.AppendAllTextAsync(_logFilePath, formattedMessage + Environment.NewLine);
                    
                    // Rotate log file if it gets too large (> 10MB)
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Exists && fileInfo.Length > 10 * 1024 * 1024)
                    {
                        RotateLogFile();
                    }
                }
                catch
                {
                    // Ignore file errors
                }
            }
        }

        private void RotateLogFile()
        {
            try
            {
                var backupPath = _logFilePath + ".old";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(_logFilePath, backupPath);
            }
            catch
            {
                MessageBox.Show("Failed to rotate log file. Logging will continue in the existing file.", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string FormatLogEntry(LogEntry logEntry)
        {
            return $"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{logEntry.Level.ToString().ToUpper().PadRight(8)}] [{logEntry.Category.PadRight(12)}] [T{logEntry.ThreadId:D2}] {logEntry.Message}";
        }

        public void FlushLogs()
        {
            // Wait for all queued logs to be processed
            var timeout = DateTime.Now.AddSeconds(5);
            while (!_logQueue.IsEmpty && DateTime.Now < timeout)
            {
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Info("Logger shutting down", "Logger");
            
            _cancellationTokenSource?.Cancel();
            
            try
            {
                _logWriterTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Ignore timeout during shutdown
            }
            
            _cancellationTokenSource?.Dispose();
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Category { get; set; }
            public string Message { get; set; }
            public int ThreadId { get; set; }
        }
    }
}