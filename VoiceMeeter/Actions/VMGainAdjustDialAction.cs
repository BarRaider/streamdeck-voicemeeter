using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.gainadjust")]
    class VMGainAdjustDialAction : EncoderBase
    {
        private enum StripBusType
        {
            Strip = 0,
            Bus = 1
        }

        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Strip = StripBusType.Strip,
                    StripNum = 0,
                    Title = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "strip")]
            public StripBusType Strip { get; set; }

            [JsonProperty(PropertyName = "stripNum")]
            public int StripNum { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }
        }

        #region Private members

        private readonly PluginSettings settings;
        private const float MIN_DB_VALUE = -60f;
        private const float MAX_DB_VALUE = 12f;

        private readonly string[] DEFAULT_IMAGES = new string[]
       {
            @"images\muteEnabled.png",
            @"images\volumeIcon@2x.png"
       };
        private string mutedImageStr;
        private string unmutedImageStr;
        private bool didSetNotConnected = false;
        private bool dialWasRotated = false;

        #endregion

        #region Public Methods

        public VMGainAdjustDialAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            PrefetchImages(DEFAULT_IMAGES);
        }

        #endregion

        #region PluginBase

        public async override void DialRotate(DialRotatePayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Rotate");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Dial Rotate but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            dialWasRotated = true;
            float.TryParse(VMManager.Instance.GetParam(BuildDeviceName()), out float volume);
            int increment = payload.Ticks;
            if (payload.IsDialPressed)
            {
                increment = 10 * (payload.Ticks > 0 ? 1 : -1);
            }
            double outputVolume = Math.Round(volume + increment);
            outputVolume = Math.Max(MIN_DB_VALUE, outputVolume);
            outputVolume = Math.Min(MAX_DB_VALUE, outputVolume);

            VMManager.Instance.SetParam(BuildDeviceName(), (float)outputVolume);
        }

        public async override void DialPress(DialPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Pressed");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Dial Pressed but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            if (payload.IsDialPressed)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Pressed");
                dialWasRotated = false;
                return;
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Released");
            if (dialWasRotated)
            {
                return;
            }

            // Run only on release and if dial wasn't rotated
            ToggleMute();
        }

        public async override void TouchPress(TouchpadPressPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Touch Pressed");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Touch Pressed but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            ToggleMute();
        }

        public async override void OnTick()
        {
            if (!VMManager.Instance.IsConnected)
            {
                await Connection.SetFeedbackAsync("icon", Properties.Plugin.Default.VMNotRunning);
                return;
            }
            else if (didSetNotConnected)
            {
                didSetNotConnected = false;
                await Connection.SetFeedbackAsync("icon", "");
            }

            string deviceName = BuildDeviceName();
            Dictionary<string, string> dkv = new Dictionary<string, string>();
            if (VMManager.Instance.GetParamBool(BuildMuteName())) // Is Muted
            {
                dkv["icon"] = mutedImageStr;
                dkv["title"] = String.IsNullOrEmpty(settings.Title) ? deviceName : settings.Title;
                dkv["value"] = "Muted";
                dkv["indicator"] = "0";
                await Connection.SetFeedbackAsync(dkv);
            }
            else
            {
                _= float.TryParse(VMManager.Instance.GetParam(BuildDeviceName()), out float gainValue);
                dkv["icon"] = unmutedImageStr;
                dkv["title"] = String.IsNullOrEmpty(settings.Title) ? deviceName : settings.Title;
                dkv["value"] =  $"{gainValue} db";
                dkv["indicator"] = Tools.RangeToPercentage((int)gainValue, (int)MIN_DB_VALUE, (int)MAX_DB_VALUE).ToString();
                await Connection.SetFeedbackAsync(dkv);
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private string BuildDeviceName()
        {
            return $"{settings.Strip}[{settings.StripNum}].gain";
        }

        private string BuildMuteName()
        {
            return $"{settings.Strip}[{settings.StripNum}].mute";
        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void PrefetchImages(string[] defaultImages)
        {
            if (defaultImages.Length < 2)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"{this.GetType()} PrefetchImages: Invalid default images list");
                return;
            }

            mutedImageStr = Tools.ImageToBase64(Image.FromFile(defaultImages[0]), true);
            unmutedImageStr = Tools.ImageToBase64(Image.FromFile(defaultImages[1]), true);
        }

        private void ToggleMute()
        {
            bool isMuted = VMManager.Instance.GetParamBool(BuildMuteName());
            VMManager.Instance.SetParam(BuildMuteName(), ((!isMuted) ? 1 : 0));
        }

        #endregion
    }
}
