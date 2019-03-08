using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.vmmicrophone")]
    class VMMicrophone : PluginBase
    {
        private enum MicTypeEnum
        {
            SingleMode = 0,
            Toggle = 1,
            PTT = 2
        }

        private enum ImageTypeEnum
        {
            Microphone = 0,
            Speaker = 1,
            OnOff = 2,
            Microphone2 = 3,
            UserDefined = 4
        }

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.MicType = MicTypeEnum.Toggle;
                instance.Strip = "Strip";
                instance.StripNum = 0;
                instance.SingleValue = String.Empty;
                instance.ImageType = ImageTypeEnum.Microphone;
                instance.UserImage1 = String.Empty;
                instance.UserImage2 = String.Empty;

                return instance;
            }
            
            [JsonProperty(PropertyName = "micType")]
            public MicTypeEnum MicType { get; set; }

            [JsonProperty(PropertyName = "strip")]
            public string Strip { get; set; }

            [JsonProperty(PropertyName = "stripNum")]
            public int StripNum { get; set; }

            [JsonProperty(PropertyName = "singleValue")]
            public string SingleValue { get; set; }

            [JsonProperty(PropertyName = "imageType")]
            public ImageTypeEnum ImageType { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "userImage1")]
            public string UserImage1 { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "userImage2")]
            public string UserImage2 { get; set; }
        }

        #region Private members

        private PluginSettings settings;

        #endregion

        #region Public Methods

        public VMMicrophone(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

            switch (settings.MicType)
            {
                case MicTypeEnum.SingleMode:
                    int value;
                    if (Int32.TryParse(settings.SingleValue, out value))
                    {
                        VMManager.Instance.SetParam(BuildDeviceName(), value);
                    }
                    else
                    {
                        await Connection.ShowAlert();
                    }
                    break;
                case MicTypeEnum.Toggle:
                    bool isMuted = VMManager.Instance.GetParamBool(BuildDeviceName());
                    VMManager.Instance.SetParam(BuildDeviceName(), isMuted ? 0 : 1);
                    break;
                case MicTypeEnum.PTT:
                    VMManager.Instance.SetParam(BuildDeviceName(), 0);
                    break;
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (settings.MicType == MicTypeEnum.PTT)
            {
                VMManager.Instance.SetParam(BuildDeviceName(), 1);
            }
        }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.SetImageAsync(Properties.Plugin.Default.VMNotRunning);
                return;
            }

            await Connection.SetImageAsync(GetBase64ImageStatus());
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

        private string GetBase64ImageStatus()
        {
            bool isMuted = VMManager.Instance.GetParamBool(BuildDeviceName());
            if (isMuted)
            {
                switch (settings.ImageType)
                {
                    case ImageTypeEnum.Microphone:
                        return Properties.Plugin.Default.MicMute;
                    case ImageTypeEnum.Speaker:
                        return Properties.Plugin.Default.SpeakerDisabled;
                    case ImageTypeEnum.OnOff:
                        return Properties.Plugin.Default.OnOffDisabled;
                    case ImageTypeEnum.Microphone2:
                        return Properties.Plugin.Default.Mic2Mute;
                    case ImageTypeEnum.UserDefined:
                        return Tools.FileToBase64(settings.UserImage1.ToString(), true);
                }

            }
            else
            {
                switch (settings.ImageType)
                {
                    case ImageTypeEnum.Microphone:
                        return Properties.Plugin.Default.MicEnabled;
                    case ImageTypeEnum.Speaker:
                        return Properties.Plugin.Default.SpeakerEnabled;
                    case ImageTypeEnum.OnOff:
                        return Properties.Plugin.Default.OnOffEnabled;
                    case ImageTypeEnum.Microphone2:
                        return Properties.Plugin.Default.Mic2Enabled;
                    case ImageTypeEnum.UserDefined:
                        return Tools.FileToBase64(settings.UserImage2.ToString(), true);
                }
            }

            return Properties.Plugin.Default.MicEnabled;
        }

        private string BuildDeviceName()
        {
            return $"{settings.Strip}[{settings.StripNum}].Mute";
        }
        #endregion
    }
}

