using System.Globalization;
using System.IO;

namespace UniversalQuickPreview.App.Logging;

public sealed class LocalFileLogger
{
    private const long MaximumLogBytes = 2 * 1024 * 1024;
    private readonly object _sync = new();

    public string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UniversalQuickPreview",
        "Logs");

    public void Info(string eventName, params (string Key, object? Value)[] properties)
    {
        Write("INF", eventName, null, properties);
    }

    public void Error(string eventName, Exception exception, params (string Key, object? Value)[] properties)
    {
        Write("ERR", eventName, exception, properties);
    }

    private void Write(
        string level,
        string eventName,
        Exception? exception,
        IReadOnlyCollection<(string Key, object? Value)> properties)
    {
        try
        {
            lock (_sync)
            {
                Directory.CreateDirectory(LogDirectory);
                var logPath = Path.Combine(LogDirectory, "UniversalQuickPreview.log");
                RotateIfNeeded(logPath);

                var values = properties.Select(property =>
                    $"{Sanitize(property.Key)}={Sanitize(Convert.ToString(property.Value, CultureInfo.InvariantCulture) ?? string.Empty)}");
                var details = string.Join(" ", values);
                if (exception is not null)
                {
                    details = $"{details} exception={Sanitize(exception.GetType().Name)} message={Sanitize(exception.Message)}".Trim();
                }

                var line = $"{DateTimeOffset.Now:O} | {level} | {Sanitize(eventName)} | {details}{Environment.NewLine}";
                File.AppendAllText(logPath, line);
            }
        }
        catch (Exception loggingException) when (loggingException is IOException or UnauthorizedAccessException)
        {
            // Logging is best-effort and must not affect preview behavior.
        }
    }

    private static void RotateIfNeeded(string logPath)
    {
        if (!File.Exists(logPath) || new FileInfo(logPath).Length < MaximumLogBytes)
        {
            return;
        }

        File.Move(logPath, logPath + ".1", overwrite: true);
    }

    private static string Sanitize(string value)
    {
        return value.Replace('\r', ' ').Replace('\n', ' ').Replace('|', '/');
    }
}
