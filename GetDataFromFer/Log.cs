using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GetDataFromFer
{
    public static class Log
    {
        private static Stopwatch _stopwatchTimer;
        private static Queue<(string data, int id, bool writeToFile)> _queue;
        private static bool _isRunning = false;
        private static Task _logger;
        private static bool _writeToFile;
        private static string _filePath;
        public static void InitConsole()
        {
            Console.Clear();
            _stopwatchTimer = new Stopwatch();
            _stopwatchTimer.Restart();
            _queue = new Queue<(string data, int id, bool writeToFile)>();
        }
        public static void Start()
        {
            _isRunning = true;
            _logger = InternalWriter();
        }
        public static void Stop()
        {
            _isRunning = false;
            _stopwatchTimer.Stop();
        }
        public static void WriteToFile(string filePath)
        {
            _writeToFile = true;
            _filePath = filePath;
        }
        
        public static void LogData(string className, string logData, int id, bool writeToFile = true)
        {
            _queue.Enqueue(($"{className} - {logData} - {Math.Round(_stopwatchTimer.Elapsed.TotalSeconds, 1)}s", id, writeToFile));
        }
        private async static Task InternalWriter()
        {
            while (_isRunning)
            {
                try
                {
                    if (_queue.Count > 0)
                    {
                        var (data, id, writeToFile) = _queue.Dequeue();
                        if (data == null)
                            continue;
                        Console.CursorTop = id;
                        Console.CursorLeft = 0;
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.CursorLeft = 0;
                        Console.Write(data.Substring(0,Math.Min(data.Length, Console.WindowWidth - 1)).Replace('\n', ' '));
                        if (_writeToFile && writeToFile)
                        {
                            System.IO.File.AppendAllText(_filePath, data + '\n');
                        }
                    }
                    else
                    {
                        await Task.Delay(500);
                        
                    }
                    
                    //LogData("Log", $"{_stopwatchTimer.Elapsed.TotalSeconds}s elappsed", Console.WindowHeight - 2, false);
                    Console.CursorTop = Console.WindowHeight - 2;
                    Console.CursorLeft = 0;
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.CursorLeft = 0;
                    var date = $"{Math.Round(_stopwatchTimer.Elapsed.TotalSeconds, 1)}s elappsed";
                    Console.Write(date.Substring(0, Math.Min(date.Length, Console.WindowWidth - 1)));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Logger error {ex}");
                }
            }
        }
    }
}
