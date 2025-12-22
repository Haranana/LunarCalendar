using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class LoggingService
    {
        private readonly string source;
        private readonly string logName;
        private EventLog eventLog;

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

        private void Write(EventLogEntryType type, string message, Exception ex)
        {
            string text = message;
            if(text.Length > 1000)
            {
                text = text.Substring(0, 1000);
            }
            if (ex != null) {
                text = text + Environment.NewLine + ex.ToString();
            }

            if (eventLog != null)
            {
                try
                {
                    eventLog.WriteEntry(text, type);
                    return;
                }
                catch
                {}
            }
        }


        public void WriteInfo(string message) 
        {
            Write(EventLogEntryType.Information, message, null);
        }
        public void WriteWarning(string message)
        { 
            Write(EventLogEntryType.Warning, message, null);
        }
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
    }
}