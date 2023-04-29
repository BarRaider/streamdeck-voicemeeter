using BarRaider.SdTools;
using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMeeter
{
    [PluginActionId("com.barraider.settingadjust")]
    class VMSettingAdjustDialAction : EncoderBase
    {
        private enum ParamTypeEnum
        {
            comp = 0,
            denoiser = 1,
            delay = 2,
            fx1 = 3,
            fx2 = 4,
            gate = 5,
            reverb = 6,
        }

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
                    ParamType = ParamTypeEnum.comp,
                    Strip = StripBusType.Strip,
                    StripNum = 0,
                    StepSize = DEFAULT_STEP_SIZE.ToString(),
                    Title = String.Empty
                };

                return instance;
            }

            [JsonProperty(PropertyName = "paramType")]
            public ParamTypeEnum ParamType { get; set; }

            [JsonProperty(PropertyName = "strip")]
            public StripBusType Strip { get; set; }

            [JsonProperty(PropertyName = "stripNum")]
            public int StripNum { get; set; }

            [JsonProperty(PropertyName = "stepSize")]
            public string StepSize { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }
        }

        #region Private members

        private const float MIN_VALUE = 0;
        private const float MAX_VALUE = 10;
        private const double DEFAULT_STEP_SIZE = 1;

        private readonly PluginSettings settings;
        private readonly string[] DEFAULT_IMAGES = new string[]
       {
            @"images\settingAdjustAction@2x.png"
       };
        private string mainImageStr;
        private bool didSetNotConnected = false;
        private bool dialWasRotated = false;
        private double stepSize = DEFAULT_STEP_SIZE;

        #endregion

        #region Public Methods

        public VMSettingAdjustDialAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            InitializeSettings();
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
            float.TryParse(VMManager.Instance.GetParam(BuildDeviceName()), out float value);
            double increment = payload.Ticks * stepSize;
            double outputValue = value + increment;
            outputValue = Math.Max(MIN_VALUE, outputValue);
            outputValue = Math.Min(MAX_VALUE, outputValue);

            VMManager.Instance.SetParam(BuildDeviceName(), (float)outputValue);
        }

        public async override void DialDown(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Down");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Dial Down but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            dialWasRotated = false;
        }

        public async override void DialUp(DialPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Dial Up");
            if (!VMManager.Instance.IsConnected)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Dial Up but VM is not connected!");
                await Connection.ShowAlert();
                return;
            }

            if (dialWasRotated)
            {
                return;
            }

            // Run only on release and if dial wasn't rotated
            DisableSetting();
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

            DisableSetting();
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

            _= float.TryParse(VMManager.Instance.GetParam(BuildDeviceName()), out float value);
            dkv["icon"] = mainImageStr;
            dkv["title"] = String.IsNullOrEmpty(settings.Title) ? deviceName : settings.Title;
            dkv["value"] =  $"{value}";
            dkv["indicator"] = Tools.RangeToPercentage((int)value, (int)MIN_VALUE, (int)MAX_VALUE).ToString();
            await Connection.SetFeedbackAsync(dkv);
        }


        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload) 
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private string BuildDeviceName()
        {
            return $"{settings.Strip}[{settings.StripNum}].{settings.ParamType}";
        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void PrefetchImages(string[] defaultImages)
        {
            if (defaultImages.Length < 1)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"{this.GetType()} PrefetchImages: Invalid default images list");
                return;
            }

            mainImageStr = Tools.ImageToBase64(Image.FromFile(defaultImages[0]), true);
        }

        private void DisableSetting()
        {
            VMManager.Instance.SetParam(BuildDeviceName(), 0);
        }

        private void InitializeSettings()
        {
            if (!double.TryParse(settings.StepSize, out stepSize))
            {
                stepSize = DEFAULT_STEP_SIZE;
                SaveSettings();
            }
        }

        #endregion
    }
}
