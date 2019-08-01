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
    [PluginActionId("com.barraider.vmadvancedtoggle")]
    class VMAdvancedToggle : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.Mode1Value = String.Empty;
                instance.Mode1Param = String.Empty;
                instance.Mode2Value = String.Empty;
                instance.TitleType = TitleTypeEnum.VMLive;
                instance.TitleParam = String.Empty;
                instance.UserImage1 = String.Empty;
                instance.UserImage2 = String.Empty;
                instance.TitlePrefix = String.Empty;
                instance.EnabledText = String.Empty;
                instance.DisabledText = String.Empty;

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
        }

        #region Private members

        private PluginSettings settings;

        #endregion

        #region Public Methods

        public VMAdvancedToggle(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        #endregion

        #region IPluginable

        public async override void KeyPressed(KeyPayload payload)
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }

            bool isMode1 = IsMode1();
            if (isMode1 && !String.IsNullOrEmpty(settings.Mode2Value))
            {
                VMManager.Instance.SetParameters(settings.Mode2Value);
            }
            // Current in Mode2
            else if(!isMode1 && !String.IsNullOrEmpty(settings.Mode1Value))
            {
                VMManager.Instance.SetParameters(settings.Mode1Value);
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.SetImageAsync(Properties.Plugin.Default.VMNotRunning);
                return;
            }

            // Set the image
            if (!String.IsNullOrEmpty(settings.UserImage1) && IsMode1())
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
                    value = settings.EnabledText;
                }
                else if (!String.IsNullOrEmpty(settings.DisabledText) && !String.IsNullOrEmpty(value) && value == Constants.DISABLED_VALUE)
                {
                    value = settings.DisabledText;
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
            // New in StreamDeck-Tools v2.0:
            Tools.AutoPopulateSettings(settings, payload.Settings);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Settings loaded: {payload.Settings}");

            // Used to return the correct filename back to the Property Inspector
            await Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private bool IsMode1()
        {
            if (!String.IsNullOrEmpty(settings.Mode1Param))
            {
                return VMManager.Instance.GetParamBool(settings.Mode1Param);
            }

            return false;
        }
        #endregion
    }
}
