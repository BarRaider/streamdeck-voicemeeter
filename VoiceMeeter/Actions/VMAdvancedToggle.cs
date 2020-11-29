using BarRaider.SdTools;
using HotkeyCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceMeeter.Midi;
using WindowsInput.Native;

namespace VoiceMeeter
{
    //---------------------------------------------------
    //          BarRaider's Hall Of Fame
    // Subscriber: Vedeksu GIFTED SUB X8!!!
    // 1700 Bits: Vedeksu
    // 1500 Bits: ovpx
    // 750 Bits: fragglerocket
    // Subscriber: fragglerocket
    // Subscriber: CyberlightGames gifted sub
    // Subscriber: icessassin gifted sub x3
    // 300 Bits: iiRoco
    // Subscriber: brandoncc2 x2 Gifted subs
    //---------------------------------------------------
    [PluginActionId("com.barraider.vmadvancedtoggle")]
    class VMAdvancedToggleAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Mode1Value = String.Empty,
                    Mode1Param = String.Empty,
                    Mode2Value = String.Empty,
                    TitleType = TitleTypeEnum.VMLive,
                    TitleParam = String.Empty,
                    UserImage1 = String.Empty,
                    UserImage2 = String.Empty,
                    TitlePrefix = String.Empty,
                    EnabledText = String.Empty,
                    DisabledText = String.Empty,
                    Mode1Hotkey = String.Empty,
                    Mode2Hotkey = String.Empty,
                    Mode1Midi = String.Empty,
                    Mode2Midi = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "mode1Value")]
            public string Mode1Value { get; set; }

            [JsonProperty(PropertyName = "mode1Param")]
            public string Mode1Param { get; set; }

            [JsonProperty(PropertyName = "mode2Value")]
            public string Mode2Value { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "userImage1")]
            public string UserImage1 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "userImage2")]
            public string UserImage2 { get; set; }

            [JsonProperty(PropertyName = "titleType")]
            public TitleTypeEnum TitleType { get; set; }

            [JsonProperty(PropertyName = "titleParam")]
            public string TitleParam { get; set; }

            [JsonProperty(PropertyName = "titlePrefix")]
            public string TitlePrefix { get; set; }

            [JsonProperty(PropertyName = "enabledText")]
            public string EnabledText { get; set; }

            [JsonProperty(PropertyName = "disabledText")]
            public string DisabledText { get; set; }

            [JsonProperty(PropertyName = "mode1Hotkey")]
            public string Mode1Hotkey { get; set; }

            [JsonProperty(PropertyName = "mode2Hotkey")]
            public string Mode2Hotkey { get; set; }

            [JsonProperty(PropertyName = "mode1Midi")]
            public string Mode1Midi { get; set; }

            [JsonProperty(PropertyName = "mode2Midi")]
            public string Mode2Midi { get; set; }
        }

        #region Private members
        private const string LOGICAL_AND = " AND ";
        private const string LOGICAL_OR = " OR ";

        private readonly PluginSettings settings;
        private bool didSetNotConnected = false;

        #endregion

        #region Public Methods

        public VMAdvancedToggleAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            InitializeSettings();
        }

        #endregion

        #region IPluginable

        public async override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} KeyPressed");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key pressed but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            bool isMode1 = IsMode1(true);
            if (isMode1) // Currently in Mode1, so run Mode2 commands
            {
                if (!String.IsNullOrEmpty(settings.Mode2Value))
                {
                    VMManager.Instance.SetParameters(settings.Mode2Value);
                }
                MidiCommandHandler.HandleMidiParameters(settings.Mode2Midi);

                if (!String.IsNullOrEmpty(settings.Mode2Hotkey))
                {
                    HotkeyHandler.RunHotkey(settings.Mode2Hotkey);
                }
            }
            else // Currently in Mode2, so run Mode1 commands
            {
                if (!String.IsNullOrEmpty(settings.Mode1Value))
                {
                    VMManager.Instance.SetParameters(settings.Mode1Value);
                }
                MidiCommandHandler.HandleMidiParameters(settings.Mode1Midi);

                if (!String.IsNullOrEmpty(settings.Mode1Hotkey))
                {
                    HotkeyHandler.RunHotkey(settings.Mode1Hotkey);
                }
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                didSetNotConnected = true;
                await Connection.SetImageAsync(Properties.Plugin.Default.VMNotRunning);
                return;
            }
            else if (didSetNotConnected)
            {
                didSetNotConnected = false;
                await Connection.SetImageAsync((String)null);
            }

            // Set the image
            if (!String.IsNullOrEmpty(settings.UserImage1) && IsMode1(false))
            {
                await Connection.SetImageAsync(Tools.FileToBase64(settings.UserImage1.ToString(), true));
            }
            else if (!String.IsNullOrEmpty(settings.UserImage2))
            {
                await Connection.SetImageAsync(Tools.FileToBase64(settings.UserImage2.ToString(), true));
            }

            // Set the title
            if (settings.TitleType == TitleTypeEnum.VMLive && !String.IsNullOrEmpty(settings.TitleParam))
            {
                string prefix = String.Empty;
                if (!String.IsNullOrEmpty(settings.TitlePrefix))
                {
                    prefix = settings.TitlePrefix.Replace(@"\n", "\n");
                }

                string value = VMManager.Instance.GetParam(settings.TitleParam);
                if (!String.IsNullOrEmpty(settings.EnabledText) && !String.IsNullOrEmpty(value) && value == Constants.ENABLED_VALUE)
                {
                    value = settings.EnabledText.Replace(@"\n", "\n"); ;
                }
                else if (!String.IsNullOrEmpty(settings.DisabledText) && !String.IsNullOrEmpty(value) && value == Constants.DISABLED_VALUE)
                {
                    value = settings.DisabledText.Replace(@"\n", "\n"); ;
                }

                await Connection.SetTitleAsync($"{prefix}{value}");
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            // Used to return the correct filename back to the Property Inspector
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private bool IsMode1(bool shouldLog)
        {
            if (String.IsNullOrEmpty(settings.Mode1Param))
            {
                return false;
            }

            // Support EITHER one, not both
            if (settings.Mode1Param.Contains(LOGICAL_AND) && settings.Mode1Param.Contains(LOGICAL_OR))
            {
                if (shouldLog)
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"Invalid IsMode1 check - contains *both* AND and OR: {settings.Mode1Param}");
                }
                return false;
            }

            string[] clauses;
            if (settings.Mode1Param.Contains(LOGICAL_AND)) // Contains ANDs
            {
                clauses = settings.Mode1Param.Split(new string[] { LOGICAL_AND }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string clause in clauses)
                {
                    // If even one of them is false, that's enough to return a false
                    if (!VMManager.Instance.GetParamBool(clause.Trim()))
                    {
                        if (shouldLog)
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"Mode1 returned false due to clause: {clause}");
                        }
                        return false;
                    }
                }
                return true;
            }
            else if (settings.Mode1Param.Contains(LOGICAL_OR)) // Contains ORs
            {
                clauses = settings.Mode1Param.Split(new string[] { LOGICAL_OR }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string clause in clauses)
                {
                    // If ANY clause is true, that's enough to return a true
                    if (VMManager.Instance.GetParamBool(clause.Trim()))
                    {
                        if (shouldLog)
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"Mode1 returned true due to clause: {clause}");
                        }
                        return true;
                    }
                }
                return false;
            }
            else // Only one clause
            {
                return VMManager.Instance.GetParamBool(settings.Mode1Param);
            }

        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void InitializeSettings()
        {
            string mode1Hotkey = HotkeyHandler.ParseKeystroke(settings.Mode1Hotkey);
            string mode2Hotkey = HotkeyHandler.ParseKeystroke(settings.Mode2Hotkey);

            // If the parsed hotkey is different than what the user inputed, overwrite the user input
            // because it's invalid
            if (mode1Hotkey != settings.Mode1Hotkey || mode2Hotkey != settings.Mode2Hotkey)
            {
                settings.Mode1Hotkey = mode1Hotkey;
                settings.Mode2Hotkey = mode2Hotkey;
                SaveSettings();
            }
        }

        #endregion
    }
}
