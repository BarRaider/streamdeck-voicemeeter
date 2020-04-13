using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace HotkeyCommands
{
    public static class HotkeyHandler
    {
        private const string MACRO_END = "}}";
        private const string REGEX_MACRO = @"^\{(\{[^\{\}]+\})+\}$";
        private const string REGEX_SUB_COMMAND = @"(\{[^\{\}]+\})";
        public static string ParseKeystroke(string keyStroke)
        {
            if (String.IsNullOrEmpty(keyStroke))
            {
                return String.Empty;
            }

            // Removed in VoiceMeeter since we expect an actual keystroke/hotkey and not just one character
            /*
            if (keyStroke.Length == 1) // 1 Character is fine
            {
                return keyStroke;
            }*/

            string macro = ExtractMacro(keyStroke, 0);
            if (string.IsNullOrEmpty(macro)) // Not a macro, save only first character
            {
                return String.Empty;
            }
            return macro;
        }

        public static void RunHotkey(string hotkey)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Simulating hotkey: {hotkey}");
            List<VirtualKeyCodeContainer> keyStrokes = HotkeyHandler.ExtractKeyStrokes(hotkey);
            VirtualKeyCodeContainer keyCode = keyStrokes.Last();
            keyStrokes.Remove(keyCode);

            if (keyStrokes.Count > 0)
            {
                Task.Run(() => SimulateKeyStroke(keyStrokes.Select(ks => ks.KeyCode).ToArray(), keyCode.KeyCode));
            }
        }

        private static void SimulateKeyStroke(VirtualKeyCode[] keyStrokes, VirtualKeyCode keyCode)
        {
            WindowsInput.InputSimulator iis = new WindowsInput.InputSimulator();
            iis.Keyboard.ModifiedKeyStroke(keyStrokes, keyCode);
        }


        internal static List<VirtualKeyCodeContainer> ExtractKeyStrokes(string macroText)
        {
            List<VirtualKeyCodeContainer> keyStrokes = new List<VirtualKeyCodeContainer>();


            try
            {
                MatchCollection matches = Regex.Matches(macroText, REGEX_SUB_COMMAND);
                foreach (var match in matches)
                {
                    string matchText = match.ToString().Replace("{", "").Replace("}", "");
                    if (matchText.Length == 1)
                    {
                        char code = matchText.ToUpperInvariant()[0];
                        keyStrokes.Add(new VirtualKeyCodeContainer((VirtualKeyCode)code));
                    }
                    else
                    {
                        VirtualKeyCodeContainer stroke = MacroTextToKeyCode(matchText);
                        if (stroke != null)
                        {
                            keyStrokes.Add(stroke);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"ExtractKeyStrokes Exception: {ex}");
            }

            return keyStrokes;
        }

        internal static string ExtractMacro(string text, int position)
        {
            try
            {
                int endPosition = text.IndexOf(MACRO_END, position);

                // Found an end, let's verify it's actually a macro
                if (endPosition > position)
                {
                    // Use Regex to verify it's really a macro
                    var match = Regex.Match(text.Substring(position, endPosition - position + MACRO_END.Length), REGEX_MACRO);
                    if (match.Length > 0)
                    {
                        return match.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"ExtractMacro Exception: {ex}");
            }

            return null;
        }

        private static VirtualKeyCodeContainer MacroTextToKeyCode(string macroText)
        {
            try
            {
                string text = ConvertSimilarMacroCommands(macroText.ToUpperInvariant());
                VirtualKeyCode keyCode = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), text, true);
                return new VirtualKeyCodeContainer(keyCode);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"MacroTextToInt Exception: {ex}");
                return null;
            }
        }

        internal static string ConvertSimilarMacroCommands(string macroText)
        {
            switch (macroText)
            {
                case "CTRL":
                    return "CONTROL";
                case "LCTRL":
                    return "LCONTROL";
                case "RCTRL":
                    return "RCONTROL";
                case "ALT":
                    return "MENU";
                case "LALT":
                    return "LMENU";
                case "RALT":
                    return "RMENU";
                case "ENTER":
                    return "RETURN";
                case "BACKSPACE":
                    return "BACK";
                case "WIN":
                    return "LWIN";
                case "WINDOWS":
                    return "LWIN";
                case "PAGEUP":
                case "PGUP":
                    return "PRIOR";
                case "PAGEDOWN":
                case "PGDN":
                    return "NEXT";
                case "BREAK":
                    return "PAUSE";

            }

            return macroText;
        }

    }
}
