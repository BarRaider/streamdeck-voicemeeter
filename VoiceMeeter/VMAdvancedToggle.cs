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
    class VMAdvancedToggle : IPluginable
    {
        private enum TitleTypeEnum
        {
            VMLive = 0,
            None = 1
        }

        private class InspectorSettings : SettingsBase
        {
            public static InspectorSettings CreateDefaultSettings()
            {
                InspectorSettings instance = new InspectorSettings();
                instance.Mode1Value = String.Empty;
                instance.Mode1Param = String.Empty;
                instance.Mode2Value = String.Empty;
                instance.TitleType = TitleTypeEnum.VMLive;
                instance.TitleParam = String.Empty;
                instance.UserImage1 = String.Empty;
                instance.UserImage2 = String.Empty;

                return instance;
            }

            [JsonProperty(PropertyName = "mode1Value")]
            public string Mode1Value { get; set; }

            [JsonProperty(PropertyName = "mode1Param")]
            public string Mode1Param { get; set; }

            [JsonProperty(PropertyName = "mode2Value")]
            public string Mode2Value { get; set; }

            [JsonProperty(PropertyName = "userImage1")]
            public string UserImage1 { get; set; }

            [JsonProperty(PropertyName = "userImage2")]
            public string UserImage2 { get; set; }

            [JsonProperty(PropertyName = "titleType")]
            public TitleTypeEnum TitleType { get; set; }

            [JsonProperty(PropertyName = "titleParam")]
            public string TitleParam { get; set; }
        }

        #region Private members

        private InspectorSettings settings;

        #endregion

        #region Public Methods

        public VMAdvancedToggle(streamdeck_client_csharp.StreamDeckConnection connection, string action, string context, JObject settings)
        {
            if (settings == null || settings.Count == 0)
            {
                this.settings = InspectorSettings.CreateDefaultSettings();
            }
            else
            {
                this.settings = settings.ToObject<InspectorSettings>();
            }

            this.settings.StreamDeckConnection = connection;
            this.settings.ActionId = action;
            this.settings.ContextId = context;
        }

        #endregion

        #region IPluginable

        public void KeyPressed()
        {
            if (!VMManager.Instance.IsConnected)
            {
                settings.ShowAlert();
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

        public void KeyReleased() {}

        public void OnTick()
        {
            // Set the image
            if (!String.IsNullOrEmpty(settings.UserImage1) && IsMode1())
            {
                settings.SetImageAsync(Tools.FileToBase64(settings.UserImage1.ToString(), true));
            }
            else if (!String.IsNullOrEmpty(settings.UserImage2))
            {
                settings.SetImageAsync(Tools.FileToBase64(settings.UserImage2.ToString(), true));
            }

            // Set the title
            if (settings.TitleType == TitleTypeEnum.VMLive && !String.IsNullOrEmpty(settings.TitleParam))
            {
                settings.SetTitleAsync(VMManager.Instance.GetParam(settings.TitleParam));
            }
        }

        public void UpdateSettings(JObject payload)
        {
            if (payload["property_inspector"] != null)
            {
                switch (payload["property_inspector"].ToString().ToLower())
                {
                    case "propertyinspectorconnected":
                        settings.SendToPropertyInspectorAsync();
                        break;

                    case "propertyinspectorwilldisappear":
                        settings.SetSettingsAsync();
                        break;

                    case "updatesettings":
                        settings.Mode1Value = (string)payload["mode1Value"];
                        settings.Mode1Param = (string)payload["mode1Param"];
                        settings.Mode2Value = (string)payload["mode2Value"];
                        settings.TitleType = (TitleTypeEnum)Enum.Parse(typeof(TitleTypeEnum), (string)payload["titleType"]);
                        settings.TitleParam = (string)payload["titleParam"];
                        settings.UserImage1 = Uri.UnescapeDataString(((string)payload["userImage1"]).Replace("C:\\fakepath\\", ""));
                        settings.UserImage2 = Uri.UnescapeDataString(((string)payload["userImage2"]).Replace("C:\\fakepath\\", ""));
                        settings.SetSettingsAsync();
                        break;
                }
            }
        }

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
