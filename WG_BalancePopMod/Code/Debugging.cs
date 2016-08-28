using System;
using System.IO;
using ColossalFramework.Plugins;
using System.Text;
using System.Collections.Generic;

namespace WG_BalancedPopMod
{
    class Debugging
    {
        private static StringBuilder sb = new StringBuilder();
        private static Dictionary<String, int> messagesToSuppress = new Dictionary<string, int>();

        // Write to a file
        public static void writeDebugToFile(String text, String fileName)
        {
            using (FileStream fs = new FileStream(ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + fileName, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(text);
            }
        }

        // Write to WG log file
        public static void writeDebugToFile(String text)
        {
            if (!messagesToSuppress.ContainsKey(text))
            {
                writeDebugToFile(text, "WG_Pop.log");
                messagesToSuppress.Add(text, 0);
            }
        }

        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine("WG_RealisticCity: " + text);
        }

        // Output buffer
        public static void releaseBuffer()
        {
            if (sb.Length > 0)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, sb.ToString());
                sb.Remove(0, sb.Length);
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
