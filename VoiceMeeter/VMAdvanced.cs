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
    class VMAdvanced : IPluginable
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
                instance.SetValue = String.Empty;
                instance.LongPressValue = String.Empty;
                instance.TitleType = TitleTypeEnum.VMLive;
                instance.TitleParam = String.Empty;

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
        }

        #region Private members

        private const int LONG_KEYPRESS_LENGTH = 1;

        private InspectorSettings settings;
        private bool keyPressed = false;
        private DateTime keyPressStart;

        #endregion

        #region Public Methods

        public VMAdvanced(streamdeck_client_csharp.StreamDeckConnection connection, string action, string context, JObject settings)
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

        public void LongKeyPressed()
        {
            if (!String.IsNullOrEmpty(settings.LongPressValue))
            {
                VMManager.Instance.SetParameters(settings.LongPressValue);
            }
        }

        #endregion

        #region IPluginable

        public void KeyPressed()
        {
            // Used for long press
            keyPressed = true;
            keyPressStart = DateTime.Now;

            if (!VMManager.Instance.IsConnected)
            {
                settings.ShowAlert();
                return;
            }

            if (!String.IsNullOrEmpty(settings.SetValue))
            {
                VMManager.Instance.SetParameters(settings.SetValue);
            }
        }

        public void KeyReleased()
        {
            keyPressed = false;
        }

        public void OnTick()
        {
            // Stream Deck calls this function every second, 
            // so this is the best place to determine if we need to call the long keypress
            if (!String.IsNullOrEmpty(settings.LongPressValue) && keyPressed && (DateTime.Now - keyPressStart).TotalSeconds > LONG_KEYPRESS_LENGTH)
            {
                LongKeyPressed();
            }

            if (settings.TitleType == TitleTypeEnum.VMLive && !String.IsNullOrEmpty(settings.TitleParam))
            {
                settings.SetTitleAsync(VMManager.Instance.GetParam(settings.TitleParam));
            }
        }

        public void Dispose()
        {
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
                        settings.SetValue = (string)payload["setValue"];
                        settings.LongPressValue = (string)payload["longPressValue"];
                        settings.TitleType = (TitleTypeEnum)Enum.Parse(typeof(TitleTypeEnum), (string)payload["titleType"]);
                        settings.TitleParam = (string)payload["titleParam"];
                        settings.SetSettingsAsync();
                        break;
                }
            }
        }

        #endregion
    }
}
