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
    [PluginActionId("com.barraider.vmadvancedptt")]
    public class VMAdvancedPTT : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.KeyPressValue = String.Empty;
                instance.KeyReleaseValue = String.Empty;
                instance.TitleType = TitleTypeEnum.VMLive;
                instance.TitleParam = String.Empty;
                instance.TitlePrefix = String.Empty;

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
        }

        #region Private members

        private PluginSettings settings;

        #endregion

        #region Public Methods

        public VMAdvancedPTT(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }

            if (!String.IsNullOrEmpty(settings.KeyPressValue))
            {
                VMManager.Instance.SetParameters(settings.KeyPressValue);
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (!String.IsNullOrEmpty(settings.KeyReleaseValue))
            {
                VMManager.Instance.SetParameters(settings.KeyReleaseValue);
            }
        }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.SetImageAsync(Properties.Plugin.Default.VMNotRunning);
                return;
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

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            // New in StreamDeck-Tools v2.0:
            Tools.AutoPopulateSettings(settings, payload.Settings);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Settings loaded: {payload.Settings}");
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        public override void Dispose() { }

        #endregion
    }
}
