using System.Runtime.CompilerServices;

namespace ToolsCore.Tools;

/// <summary>
///     Trieda sluziaca na logovanie do suborov.
/// </summary>
public static class Log
{
    private static LogFile? _logApp;
    private static LogFile? _logError;

    /// <summary>
    ///     Nazov logovacieho suboru Info.log.
    /// </summary>
    private const string LogAppName = "info.log";

    /// <summary>
    ///     Nazov logovacieho suboru Error.log.
    /// </summary>
    private const string LogErrorName = "error.log";

    /// <summary>
    ///     Cesta k priecinku s logmi.
    /// </summary>
    private const string LogPath = "\\logs";

    /// <summary>
    ///     Cesta k priecinku, do ktoreho sa zapisuju logy (podpriecinok <c>logs</c>).
    /// </summary>
    /// <remarks>
    ///     Nastavuje ju <c>AppInit.Initialization</c> na <c>AppPaths.DataDir</c>, a to hned
    ///     na zaciatku - inak by prve zalogovanie zalozilo <c>logs</c> vedla programu.
    /// </remarks>
    public static string DataDirPath { get; set; } = Application.StartupPath;

    /// <summary>
    ///     Vykonavaj logy informacii.
    /// </summary>
    public static bool DoAppLogs { get; set; }  = true;

    /// <summary>
    ///     Vykonavaj logy chyb a vynimiek.
    /// </summary>
    public static bool DoErrorLogs { get; set; } = true;

    private static void LogString(LogFile logFile, string? text) => logFile.SaveToFile(text);

    /// <summary>
    ///     Zapise chybu do logovacieho suboru.
    /// </summary>
    /// <param name="s">text chybovej hlasky</param>
    public static void Error(string? s)
    {
        if (DoErrorLogs)
        {
            _logError ??= new LogFile(LogErrorName, LogFile.DateType.Datetime);
            LogString(_logError, s);
        }
    }

    /// <summary>
    ///     Zapise vynimku do logovacieho suboru.
    /// </summary>
    /// <param name="e">Vynimka.</param>
    /// <param name="s">Dobrovodna informacia o vynimke.</param>
    public static void Exception(Exception? e, string? s = null)
    {
        if (e != null)
        {
            s = s == null ? $"{e}\r\n" : $"{s}\r\n{e}\r\n";
        }

        Error(s);
    }

    /// <summary>
    ///     Zapise informaciu do logovacieho suboru.
    /// </summary>
    /// <param name="s">Text informacie.</param>
    public static void Info(string s)
    {
        if (!DoAppLogs) 
            return;

        _logApp ??= new LogFile(LogAppName, LogFile.DateType.Datetime);
        LogString(_logApp, s);
    }

    internal class LogFile
    {
        private readonly string _dateSeparator;
        private readonly DateType _dateTypeStamp;
        private readonly object _locker;
        private readonly int _maxSize;
        private DateTime _lastDate;

        public LogFile(string fileName, DateType dateType, string dateSeparator = "\t", int maxsize = 1000000)
        {
            FileName = fileName;
            _dateTypeStamp = dateType;
            _dateSeparator = dateSeparator;
            _locker = RuntimeHelpers.GetObjectValue(new object());

            FullFileDir = Utils.CombinePath(DataDirPath, LogPath)!;
            FullFilePath = Utils.CombinePath(DataDirPath, LogPath, fileName)!;

            if (!Directory.Exists(FullFileDir)) 
                Directory.CreateDirectory(FullFileDir);

            _maxSize = maxsize;
        }

        private string FileName { get; }
        private string FullFilePath { get; }
        private string FullFileDir { get; }

        public void SaveToFile(string? text)
        {
            if (text == null) 
                return;

            lock (_locker)
            {
                var now = DateTime.Now;

                if (_maxSize > 0 && File.Exists(FullFilePath) && _maxSize < new FileInfo(FullFilePath).Length)
                {
                    var newFile = Path.GetFileNameWithoutExtension(FileName) + "_" + now.ToString("dd-MM") + ".log.bak";
                    File.Move(FullFilePath, Utils.CombinePath(FullFileDir, newFile)!);
                }

                switch (_dateTypeStamp)
                {
                    case DateType.Date:
                        text = now.ToString("dd.MM.yyyy") + _dateSeparator + text;
                        break;
                    case DateType.Datetime:
                        text = now.ToString("dd.MM.yyyy HH:mm:ss") + _dateSeparator + text;
                        break;
                    case DateType.DatetimeMs:
                        text = now.ToString("dd.MM.yyyy HH:mm:ss.fff") + _dateSeparator + text;
                        break;
                    case DateType.Time:
                        text = now.ToString("HH:mm:ss") + _dateSeparator + text;
                        break;
                    case DateType.TimeMs:
                        text = now.ToString("HH:mm:ss.fff") + _dateSeparator + text;
                        break;
                    case DateType.DateLast:
                    {
                        if (_lastDate.Equals(now.Date))
                            text = "--- " + now.ToString("dd.MM.yyyy") + " ---\r\n" + text;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException("Neznámy parameter " + _dateTypeStamp);
                }

                text += "\r\n";

                _lastDate = now.Date;

                File.AppendAllText(FullFilePath, text);
            }
        }

        public enum DateType
        {
            Date,
            Datetime,
            DatetimeMs,
            Time,
            TimeMs,
            DateLast
        }
    }
}