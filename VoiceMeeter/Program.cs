using BarRaider.SdTools;
using CommandLine;
using System;
using System.Collections.Generic;

namespace VoiceMeeter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            List<PluginActionId> supportedActionIds = new List<PluginActionId>();
            supportedActionIds.Add(new PluginActionId("com.barraider.vmmicrophone", typeof(VMMicrophone)));
            supportedActionIds.Add(new PluginActionId("com.barraider.vmmodify", typeof(VMModify)));
            supportedActionIds.Add(new PluginActionId("com.barraider.vmadvanced", typeof(VMAdvanced)));
            supportedActionIds.Add(new PluginActionId("com.barraider.vmadvancedtoggle", typeof(VMAdvancedToggle)));

            SDWrapper.Run(args, supportedActionIds.ToArray());
        }
    }
}
