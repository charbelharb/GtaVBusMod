using System;
using System.IO;

namespace GtaVBusMod.Tools
{
    public class DevToolBusLogger : IGtaVBusLogging
    {
        private const string LogPath = "BusModGtaV.log";
        
        public void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath, $"[LOG] -- {DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception )
            {
                GTA.UI.Notification.Show("Error Writing to log file");
            }
        }
    }
}