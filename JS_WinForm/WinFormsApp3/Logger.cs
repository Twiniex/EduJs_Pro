using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp3
{
    class Logger
    {
        private static Logger _instance;
        public static Logger GetInstance(string logFilePath = "log.txt")
        {
            if (_instance == null)
            {
                _instance = new Logger(logFilePath);
            }
            return _instance;
        }
        private StreamWriter _writer;
        private string _logFilePath;
        private Logger(string logFilePath)
        {
            _logFilePath = logFilePath;
            _writer = new StreamWriter(_logFilePath, true) { AutoFlush = true };
        }
        public void Log(string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd:HH:mm:ss}[INFO]{message}";
            Debug.WriteLine(logMessage);
            _writer.WriteLine(logMessage);
        }
        public void LogError(string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd:HH:mm:ss}[ERROR]{message}";
            Debug.WriteLine(logMessage);
            _writer.WriteLine(logMessage);
        }
        public void Close() => _writer?.Close();
    }
}
