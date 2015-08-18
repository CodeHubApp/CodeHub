using Splat;

namespace CodeHub.Core.Utilities
{
    #if DEBUG
    public class DiagnosticLogger : ILogger
    {
        private LogLevel _logLevel = LogLevel.Warn;

        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel >= Level)
                System.Diagnostics.Debug.WriteLine("[{0}] {1}", logLevel, message);
        }

        public LogLevel Level
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }
    }
    #endif
}

