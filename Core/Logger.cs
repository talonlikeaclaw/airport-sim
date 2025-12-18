namespace AirportSimulator.Core;

public sealed class Logger
{
    private static Logger? _instance;
    private static readonly object _lock = new object();
    private readonly string _logFile;

    private Logger()
    {
        string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AirportSimulator"
        );
        Directory.CreateDirectory(logDir);

        _logFile = Path.Combine(logDir, $"airport_{DateTime.Now:yyyy-MM-dd}.log");

        Info("Logger initialized");
    }

    public static Logger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new Logger();
                }
            }
            return _instance;
        }
    }

    public void Info(string message)
    {
        Log("INFO", message, ConsoleColor.Cyan);
    }

    public void Warning(string message)
    {
        Log("WARN", message, ConsoleColor.Yellow);
    }

    public void Error(string message, Exception? ex = null)
    {
        string fullMessage = ex != null ? $"{message} | Exception: {ex.Message}" : message;
        Log("ERROR", fullMessage, ConsoleColor.Red);
    }

    public void Success(string message)
    {
        Log("SUCCESS", message, ConsoleColor.Green);
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] {level}: {message}";

        try
        {
            // Write to file
            File.AppendAllText(_logFile, logEntry + Environment.NewLine);

            // Write to console with color
            Console.ForegroundColor = color;
            Console.WriteLine($"[{level}] {message}");
            Console.ResetColor();
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}
