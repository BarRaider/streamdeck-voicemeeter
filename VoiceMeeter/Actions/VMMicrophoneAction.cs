using BarRaider.SdTools;
using HotkeyCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.vmmicrophone")]
    class VMMicrophoneAction : PluginBase
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
                PluginSettings instance = new PluginSettings
                {
                    MicType = MicTypeEnum.Toggle,
                    Strip = "Strip",
                    StripNum = 0,
                    SingleValue = String.Empty,
                    ImageType = ImageTypeEnum.Microphone,
                    UserImage1 = String.Empty,
                    UserImage2 = String.Empty,
                    MuteHotkey = String.Empty,
                    UnmuteHotkey = String.Empty
                };

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

            [JsonProperty(PropertyName = "muteHotkey")]
            public string MuteHotkey { get; set; }

            [JsonProperty(PropertyName = "unmuteHotkey")]
            public string UnmuteHotkey { get; set; }

        }

        #region Private members

        private readonly PluginSettings settings;
        private bool didSetNotConnected = false;

        #endregion

        #region Public Methods

        public VMMicrophoneAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} KeyPressed");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key pressed but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            bool triggeredMute = false;
            switch (settings.MicType)
            {
                case MicTypeEnum.SingleMode:
                    int value;
                    if (Int32.TryParse(settings.SingleValue, out value))
                    {
                        VMManager.Instance.SetParam(BuildDeviceName(), value);
                        triggeredMute = value != 0;
                    }
                    else
                    {
                        await Connection.ShowAlert();
                    }
                    break;
                case MicTypeEnum.Toggle:
                    bool isMuted = VMManager.Instance.GetParamBool(BuildDeviceName());
                    VMManager.Instance.SetParam(BuildDeviceName(), isMuted ? 0 : 1);
                    triggeredMute = !isMuted;
                    break;
                case MicTypeEnum.PTT:
                    VMManager.Instance.SetParam(BuildDeviceName(), 0);
                    triggeredMute = false;
                    break;
            }

            if (triggeredMute && !String.IsNullOrEmpty(settings.MuteHotkey))
            {
                HotkeyHandler.RunHotkey(settings.MuteHotkey);
            }
            else if (!triggeredMute && !String.IsNullOrEmpty(settings.UnmuteHotkey))
            {
                HotkeyHandler.RunHotkey(settings.UnmuteHotkey);
            }
            
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (settings.MicType == MicTypeEnum.PTT)
            {
                VMManager.Instance.SetParam(BuildDeviceName(), 1);

                if (!String.IsNullOrEmpty(settings.MuteHotkey))
                {
                    HotkeyHandler.RunHotkey(settings.MuteHotkey);
                }
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

