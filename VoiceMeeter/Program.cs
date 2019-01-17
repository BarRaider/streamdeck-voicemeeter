﻿using BarRaider.SdTools;
using CommandLine;
using System;

namespace VoiceMeeter
{
    class Program
    {
        /************************************************************************
         * Initial configuration copied from TyrenDe's streamdeck-client-csharp example:
         * https://github.com/TyrenDe/streamdeck-client-csharp
         * and SaviorXTanren's MixItUp.StreamDeckPlugin:
         * https://github.com/SaviorXTanren/mixer-mixitup/
         * VoiceMeeter wrapper modified from original version by tocklime:
         * https://github.com/tocklime/VoiceMeeterWrapper
         *************************************************************************/

        // Handles all the communication with the plugin
        private static PluginContainer container;

        // StreamDeck launches the plugin with these details
        // -port [number] -pluginUUID [GUID] -registerEvent [string?] -info [json]
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            // The command line args parser expects all args to use `--`, so, let's append
            for (int count = 0; count < args.Length; count++)
            {
                if (args[count].StartsWith("-") && !args[count].StartsWith("--"))
                {
                    args[count] = $"-{args[count]}";
                }
            }

            Parser parser = new Parser((with) =>
            {
                    with.EnableDashDash = true;
                    with.CaseInsensitiveEnumValues = true;
                    with.CaseSensitive = false;
                    with.IgnoreUnknownArguments = true;
                    with.HelpWriter = Console.Error;
            });

            ParserResult<StreamDeckOptions> options = parser.ParseArguments<StreamDeckOptions>(args);
            options.WithParsed<StreamDeckOptions>(o => RunPlugin(o));
        }

        static void RunPlugin(StreamDeckOptions options)
        {
            container = new PluginContainer();
            container.Run(options);
        }
    }
}
