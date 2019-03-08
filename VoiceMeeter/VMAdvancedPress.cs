using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.vmadvanced")]
    class VMAdvancedPress : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.SetValue = String.Empty;
                instance.LongPressValue = String.Empty;
                instance.TitleType = TitleTypeEnum.VMLive;
                instance.TitleParam = String.Empty;
                instance.TitlePrefix = String.Empty;

                return instance;
            }

            [JsonProperty(PropertyName = "setValue")]
            public string SetValue { get; set; }

            [JsonProperty(PropertyName = "longPressValue")]
            public string LongPressValue { get; set; }

            [JsonProperty(PropertyName = "titleType")]
            public TitleTypeEnum TitleType { get; set; }

            [JsonProperty(PropertyName = "titleParam")]
            public string TitleParam { get; set; }

            [JsonProperty(PropertyName = "titlePrefix")]
            public string TitlePrefix{ get; set; }
        }

        #region Private members

        private const int LONG_KEYPRESS_LENGTH = 1;

        private PluginSettings settings;
        private bool keyPressed = false;
        private DateTime keyPressStart;

        #endregion

        #region Public Methods

        public VMAdvancedPress(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
        }

        public void LongKeyPressed()
        {
            if (!String.IsNullOrEmpty(settings.LongPressValue))
            {
                VMManager.Instance.SetParameters(settings.LongPressValue);
            }
        }

        #endregion

        #region PluginBase

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            // New in StreamDeck-Tools v2.0:
            Tools.AutoPopulateSettings(settings, payload.Settings);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Settings loaded: {payload.Settings}");
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            // Used for long press
            keyPressed = true;
            keyPressStart = DateTime.Now;

            if (!VMManager.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }

            if (!String.IsNullOrEmpty(settings.SetValue))
            {
                VMManager.Instance.SetParameters(settings.SetValue);
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            keyPressed = false;
        }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.SetImageAsync(Properties.Plugin.Default.VMNotRunning);
                return;
            }

            // Stream Deck calls this function every second, 
            // so this is the best place to determine if we need to call the long keypress
            if (!String.IsNullOrEmpty(settings.LongPressValue) && keyPressed && (DateTime.Now - keyPressStart).TotalSeconds > LONG_KEYPRESS_LENGTH)
            {
                LongKeyPressed();
            }

            if (settings.TitleType == TitleTypeEnum.VMLive && !String.IsNullOrEmpty(settings.TitleParam))
            {
                string prefix = String.Empty;
                if (!String.IsNullOrEmpty(settings.TitlePrefix))
                {
                    prefix = settings.TitlePrefix.Replace(@"\n", "\n");
                }

                await Connection.SetTitleAsync($"{prefix}{VMManager.Instance.GetParam(settings.TitleParam)}");
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion
    }
}
