using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace VoiceMeeter
{
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

            [JsonProperty(PropertyName = "userImage1")]
            public string UserImage1 { get; set; }

            [JsonProperty(PropertyName = "userImage2")]
            public string UserImage2 { get; set; }
        }

        #region Private members

        private PluginSettings settings;

        #endregion

        #region Public Methods

        public VMMicrophone(SDConnection connection, JObject settings) : base(connection, settings)
        {
            if (settings == null || settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = settings.ToObject<PluginSettings>();
            }
        }

        #endregion

        #region IPluginable

        public async override void KeyPressed()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.ShowAlert();
                return;
            }

            switch (settings.MicType)
            {
                case MicTypeEnum.SingleMode:
                    VMManager.Instance.SetParam(BuildDeviceName(), Convert.ToInt16(settings.SingleValue));
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

        public override void KeyReleased()
        {
            if (settings.MicType == MicTypeEnum.PTT)
            {
                VMManager.Instance.SetParam(BuildDeviceName(), 1);
            }
        }

        public async override void OnTick()
        {
            await Connection.SetImageAsync(GetBase64ImageStatus());
        }

        public override void Dispose()
        {
        }

        public async override void UpdateSettings(JObject payload)
        {
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString().ToLower())
                {
                    case "propertyinspectorconnected":
                        await Connection.SendToPropertyInspectorAsync(JObject.FromObject(settings));
                        break;

                    case "propertyinspectorwilldisappear":
                        await Connection.SetSettingsAsync(JObject.FromObject(settings));
                        break;

                    case "updatesettings":
                        settings.MicType     = (MicTypeEnum)Enum.Parse(typeof(MicTypeEnum), (string)payload["micType"]);
                        settings.Strip       = (string)payload["strip"];
                        settings.StripNum    = (int)payload["stripNum"];
                        settings.SingleValue = (string)payload["singleValue"];
                        settings.ImageType   = (ImageTypeEnum)Enum.Parse(typeof(ImageTypeEnum), (string)payload["imageType"]);
                        settings.UserImage1 = Uri.UnescapeDataString(((string)payload["userImage1"]).Replace("C:\\fakepath\\", ""));
                        settings.UserImage2 = Uri.UnescapeDataString(((string)payload["userImage2"]).Replace("C:\\fakepath\\", ""));
                        await Connection.SetSettingsAsync(JObject.FromObject(settings));
                        break;
                }
            }
        }

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

