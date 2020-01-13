using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TacitCoreDemo.Services
{

    /// <summary>
    /// Logs Manager
    /// </summary>
    public class LoggerManager
    {
        /// <summary>
        /// 
        /// </summary>
        public static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Information log 
        /// </summary>
        /// <param name="Message"></param>
        public static void InfoLog(string Message)
        {
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, LoggerManager._logger.Name, Message);
            _logger.Log(theEvent);
        }

        /// <summary>
        /// Debug Log
        /// </summary>
        /// <param name="Message"></param>
        public static void DebugLog(string Message)
        {
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, LoggerManager._logger.Name, Message);
            _logger.Log(theEvent);
        }

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="Message"></param>
        public static void ErrorLog(string Message)
        {
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Error, LoggerManager._logger.Name, Message);
            _logger.Log(theEvent);
        }

    }
}
