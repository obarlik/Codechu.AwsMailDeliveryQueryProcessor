using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace Codechu.AwsMailDeliveryQueryProcessor.Sample
{
    [Flags]
    enum AwsLogLevel
    {
        None = 0,
        Critical = 1,
        Error = 2 | Critical,
        Warning = 4 | Error,
        Info = 8 | Warning,
        Trace = 16 | Info,
        Debug = 32 | Warning,
        Verbose = Trace | Debug
    }

    internal class Logger : IAwsLogger
    {
        private EventLog EventLog;

        public AwsLogLevel LogLevel { get; set; }


        public Logger(EventLog eventLog)
        {
            EventLog = eventLog;

            LogLevel = ConfigurationManager.AppSettings["LogLevel"]
                .Split(',')
                .Select(s =>
                {
                    switch (s.Trim().ToLowerInvariant())
                    {
                        case "none": return AwsLogLevel.None;
                        case "critical": return AwsLogLevel.Critical;
                        case "error": return AwsLogLevel.Error;
                        case "warning": return AwsLogLevel.Warning;
                        case "info": return AwsLogLevel.Info;
                        case "trace": return AwsLogLevel.Trace;
                        case "debug": return AwsLogLevel.Debug;
                        case "verbose": return AwsLogLevel.Verbose;
                    }
                    return AwsLogLevel.None;
                })
                .Aggregate((acc, val) => acc | val);
        }

        public void LogCritical(int eventId, string message)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Critical))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine(message, $"Critical-{eventId}");
            else
                EventLog.WriteEntry(message, EventLogEntryType.Error, eventId);
        }

        public void LogDebug(int eventId, string message, Exception exception)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Debug))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine($"{message}, Error = {exception}", $"Debug-{eventId}");
            else
                EventLog.WriteEntry($"{message}, Error = {exception}", EventLogEntryType.Error, eventId);
        }

        public void LogError(int eventId, string message)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Error))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine(message, $"Error-{eventId}");
            else
                EventLog.WriteEntry(message, EventLogEntryType.Error, eventId);
        }

        public void LogInfo(int eventId, string message)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Info))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine(message, $"Information-{eventId}");
            else
                EventLog.WriteEntry(message, EventLogEntryType.Information, eventId);
        }

        public void LogTrace(int eventId, string message)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Trace))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine(message, $"Trace-{eventId}");
            else
                EventLog.WriteEntry(message, EventLogEntryType.Information, eventId);
        }

        public void LogWarning(int eventId, string message)
        {
            if (!LogLevel.HasFlag(AwsLogLevel.Warning))
                return;

            if (Debugger.IsAttached)
                Debug.WriteLine(message, $"Warning-{eventId}");
            else
                EventLog.WriteEntry(message, EventLogEntryType.Warning, eventId);
        }
    }
}