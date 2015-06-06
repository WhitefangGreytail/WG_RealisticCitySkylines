using System;
using System.IO;
using ColossalFramework.Plugins;

namespace WG_BalancedPopMod
{
    class Debugging
    {
        // Write to WG log file
        public static void writeDebugToFile(String text)
        {
            using (FileStream fs = new FileStream(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "WG_log.log", FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(text);
            }
        }

        // Write a message to the panel
        public static void panelMessage(string text)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "WG_RealisticCity: " + text);
        }


        // Write a warning to the panel
        public static void panelWarning(string text)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "WG_RealisticCity: " + text);
        }


        // Write an error to the panel
        public static void panelError(string text)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "WG_RealisticCity: " + text);
        }

    }
}
