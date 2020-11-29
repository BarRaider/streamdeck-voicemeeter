using BarRaider.SdTools;
using HotkeyCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceMeeter.Midi;
using WindowsInput.Native;

namespace VoiceMeeter
{
    //---------------------------------------------------
    //          BarRaider's Hall Of Fame
    // Subscriber: JustVoxPop
    // Subscriber: iMackx
    // Subscriber: CyberlightGames
    // Craigs_Cave - 5 Gifted Subs
    // 100 Bits: iMackx
    // JustVoxPop - 1 Gifted Subs
    // iMackx - 1 Gifted Subs
    // nubby_ninja - 5 Gifted Subs
    // 500 Bits: MagicGuitarist
    // Subscriber: SaintG85
    // Subscriber: MagicGuitarist
    // 17 Bits: TwilightLinkable
    //---------------------------------------------------
    [PluginActionId("com.barraider.vmmacrotoggle")]
    class VMMacroToggle : PluginBase
    {
        private enum MacroToggleMode
        {
            Toggle = 0,
            PTT = 1
        }
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    EnabledImage = String.Empty,
                    DisabledImage = String.Empty,
                    TitlePrefix = String.Empty,
                    EnabledText = String.Empty,
                    DisabledText = String.Empty,
                    ButtonId = DEFAULT_BUTTON_ID.ToString(),
                    ToggleMode = MacroToggleMode.Toggle
                };

                return instance;
            }

            [JsonProperty(PropertyName = "buttonId")]
            public string ButtonId { get; set; }

            [JsonProperty(PropertyName = "toggleMode")]
            public MacroToggleMode ToggleMode { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "enabledImage")]
            public string EnabledImage { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "disabledImage")]
            public string DisabledImage { get; set; }

            [JsonProperty(PropertyName = "titlePrefix")]
            public string TitlePrefix { get; set; }

            [JsonProperty(PropertyName = "enabledText")]
            public string EnabledText { get; set; }

            [JsonProperty(PropertyName = "disabledText")]
            public string DisabledText { get; set; }
        }

        #region Private members
        private const int DEFAULT_BUTTON_ID = 0;
        private static readonly string[] DEFAULT_BUTTON_IMAGES = { @"images\buttonDisabled.png", @"images\buttonEnabled.png" };

        private readonly PluginSettings settings;
        private int buttonId = DEFAULT_BUTTON_ID;
        private bool isButtonEnabled = false;
        private bool startingToggleMode = false;
        private Image enabledImage = null;
        private Image disabledImage = null;
        private bool didSetNotConnected = false;

        #endregion

        #region Public Methods

        public VMMacroToggle(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            InitializeSettings();
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

            if (String.IsNullOrEmpty(settings.ButtonId))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key pressed but ButtonID is null!");
                await Connection.ShowAlert();
                return;
            }

            // Store the initial value so we can revert back to it
            if (settings.ToggleMode == MacroToggleMode.PTT)
            {
                startingToggleMode = isButtonEnabled;
            }

            VMManager.Instance.SetMacroStatus(buttonId, !isButtonEnabled);

            if (settings.ToggleMode == MacroToggleMode.Toggle)
            {
                await Connection.ShowOk();
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (settings.ToggleMode == MacroToggleMode.PTT)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} Key Released");
                if (!VMManager.Instance.IsConnected)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key released but VM is not connected!");
                    return;
                }

                if (String.IsNullOrEmpty(settings.ButtonId))
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"Key released but ButtonID is null!");
                    return;
                }
                VMManager.Instance.SetMacroStatus(buttonId, startingToggleMode);
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

            isButtonEnabled = VMManager.Instance.GetMacroStatus(buttonId);

            // Set the image
            if (isButtonEnabled)
            {
                await Connection.SetImageAsync(enabledImage);
            }
            else
            {
                await Connection.SetImageAsync(disabledImage);
            }

            // Set the title
            string prefix = String.Empty;
            if (!String.IsNullOrEmpty(settings.TitlePrefix))
            {
                prefix = settings.TitlePrefix.Replace(@"\n", "\n");
            }

            string value = isButtonEnabled ? "1" : "0";
            if (!String.IsNullOrEmpty(settings.EnabledText) && !String.IsNullOrEmpty(value) && value == Constants.ENABLED_VALUE)
            {
                value = settings.EnabledText.Replace(@"\n", "\n"); ;
            }
            else if (!String.IsNullOrEmpty(settings.DisabledText) && !String.IsNullOrEmpty(value) && value == Constants.DISABLED_VALUE)
            {
                value = settings.DisabledText.Replace(@"\n", "\n"); ;
            }

            await Connection.SetTitleAsync($"{prefix}{value}");

        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public async override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            // Used to return the correct filename back to the Property Inspector
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #endregion

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void InitializeSettings()
        {
            if (!Int32.TryParse(settings.ButtonId, out buttonId))
            {
                settings.ButtonId = DEFAULT_BUTTON_ID.ToString();
                buttonId = DEFAULT_BUTTON_ID;
                SaveSettings();
            }

            PrefetchImages();
        }

        private void PrefetchImages()
        {
            try
            {
                if (enabledImage != null)
                {
                    enabledImage.Dispose();
                    enabledImage = null;
                }

                if (disabledImage != null)
                {
                    disabledImage.Dispose();
                    disabledImage = null;
                }

                enabledImage = Image.FromFile(TryGetCustomFile(settings.EnabledImage, DEFAULT_BUTTON_IMAGES[1]));
                disabledImage = Image.FromFile(TryGetCustomFile(settings.DisabledImage, DEFAULT_BUTTON_IMAGES[0]));
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"PrefetchImages Exception {ex}");
            }
        }
        private string TryGetCustomFile(string fileName, string fallbackFilename)
        {
            string value = fallbackFilename;
            if (!String.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName))
                {
                    Logger.Instance.LogMessage(TracingLevel.WARN, $"TryGetCustomFile: File not found {fileName}");
                }
                else
                {
                    value = fileName;
                }
            }

            return value;
        }

        #endregion
    }
}
