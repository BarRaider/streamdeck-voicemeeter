using Newtonsoft.Json.Linq;

namespace VoiceMeeter
{
    public interface IPluginable
    {
        void KeyPressed();
        void KeyReleased();
        void UpdateSettings(JObject payload);
        void OnTick();
    }
}
