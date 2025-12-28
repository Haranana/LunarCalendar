using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Writes application logs to Windows Event Log
    /// </summary>
    /// <remarks>
    /// This logger intentionally do not throws any errors related to event log access such as (missing permissions, missing source, invalid log name etc.) .
    /// The written entry is trimmed to 10 000 characters.
    /// </remarks>
    public class LoggingService : IDisposable
    {
        private readonly string source;
        private readonly string logName;
        private readonly EventLog eventLog;

        public LoggingService(string source, string logName)
        {
            this.source = source;
            this.logName = logName;

            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, logName);
                }
            }
            catch
            {
               
            }
 

            try
            {
                eventLog = new EventLog(logName) { 
                    Source = source
                };
            }
            catch
            {
                eventLog = null;

            }
        }

        /// <summary>
        /// Writes an informational entry
        /// </summary>
        public void WriteInfo(string message) 
        {
            Write(EventLogEntryType.Information, message, null);
        }

        /// <summary>
        /// Writes a warning entry
        /// </summary>
        public void WriteWarning(string message)
        { 
            Write(EventLogEntryType.Warning, message, null);
        }

        /// <summary>
        /// Writes an error entry, if <paramref name="ex"/> is provided its details are added to the message
        /// </summary>
        public void WriteError(string message, Exception ex = null)
        {
            Write(EventLogEntryType.Error, message, ex);
        }

        public void Dispose()
        {
            try { 
                eventLog?.Dispose(); 
            } catch { 
            
            }
        }

        private void Write(EventLogEntryType type, string message, Exception ex)
        {
            string text = message;
            if (ex != null)
            {
                text = text + Environment.NewLine + ex.ToString() + ex;
            }
            if (text.Length > 10000)
            {
                text = text.Substring(0, 10000);
            }

            if (eventLog != null)
            {
                try
                {
                    eventLog.WriteEntry(text, type);
                    return;
                }
                catch
                { }
            }
        }

    }
}