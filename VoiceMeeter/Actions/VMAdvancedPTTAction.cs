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

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.vmadvancedptt")]
    public class VMAdvancedPTTAction : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    KeyPressValue = String.Empty,
                    KeyReleaseValue = String.Empty,
                    TitleType = TitleTypeEnum.VMLive,
                    TitleParam = String.Empty,
                    TitlePrefix = String.Empty,
                    EnabledText = String.Empty,
                    DisabledText = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "keyPressValue")]
            public string KeyPressValue { get; set; }

            [JsonProperty(PropertyName = "keyReleaseValue")]
            public string KeyReleaseValue { get; set; }

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

            [JsonProperty(PropertyName = "keypressHotkey")]
            public string KeypressHotkey { get; set; }

            [JsonProperty(PropertyName = "keypressMidi")]
            public string KeypressMidi { get; set; }

            [JsonProperty(PropertyName = "releaseHotkey")]
            public string ReleaseHotkey { get; set; }

            [JsonProperty(PropertyName = "releaseMidi")]
            public string ReleaseMidi { get; set; }
        }

        #region Private members

        private readonly PluginSettings settings;
        private bool didSetNotConnected = false;

        #endregion

        #region Public Methods

        public VMAdvancedPTTAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public async override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} KeyPressed");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key pressed but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            if (!String.IsNullOrEmpty(settings.KeyPressValue))
            {
                VMManager.Instance.SetParameters(settings.KeyPressValue);
            }
            MidiCommandHandler.HandleMidiParameters(settings.KeypressMidi);

            if (!String.IsNullOrEmpty(settings.KeypressHotkey))
            {
                HotkeyHandler.RunHotkey(settings.KeypressHotkey);
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (!String.IsNullOrEmpty(settings.KeyReleaseValue))
            {
                VMManager.Instance.SetParameters(settings.KeyReleaseValue);
            }
            MidiCommandHandler.HandleMidiParameters(settings.ReleaseMidi);

            if (!String.IsNullOrEmpty(settings.ReleaseHotkey))
            {
                HotkeyHandler.RunHotkey(settings.ReleaseHotkey);
            }
        }

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
                    value = settings.EnabledText;
                }
                else if (!String.IsNullOrEmpty(settings.DisabledText) && !String.IsNullOrEmpty(value) && value == Constants.DISABLED_VALUE)
                {
                    value = settings.DisabledText;
                }

                await Connection.SetTitleAsync($"{prefix}{value}");
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        #endregion

        #region Private Methods

        private void InitializeSettings()
        {
            string keypressHotkey = HotkeyHandler.ParseKeystroke(settings.KeypressHotkey);
            string releaseHotkey = HotkeyHandler.ParseKeystroke(settings.ReleaseHotkey);

            // If the parsed hotkey is different than what the user inputed, overwrite the user input
            // because it's invalid
            if (keypressHotkey != settings.KeypressHotkey || releaseHotkey != settings.ReleaseHotkey)
            {
                settings.KeypressHotkey = keypressHotkey;
                settings.ReleaseHotkey = releaseHotkey;
                SaveSettings();
            }
        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion
    }
}
