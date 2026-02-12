namespace GtaVBusMod.Tools
{
    public class DevToolBusLogger : IGtaVBusLogging
    {
        public void Log(string message)
        {
            GTA.UI.Screen.ShowSubtitle(message);
        }
    }
}