using Microsoft.Win32;
using System;
using System.Text;
using BarRaider.SdTools;
using VoiceMeeter;

namespace VoiceMeeterWrapper
{
    public class VmClient : IDisposable
    {
        public VbLoginResponse LoginResponse { get; private set; }
        private Action _onClose = null;

        private readonly string[] VM_UNINSTALL_REG_LOCATIONS = { @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                                                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"};
        const string VM_UNINSTALL_KEY = "VB:Voicemeeter {17359A74-1236-5467}";
        

        private string GetVoicemeeterDir()
        {
            foreach (var regKey in VM_UNINSTALL_REG_LOCATIONS)
            {
                var key = $"{regKey}\\{VM_UNINSTALL_KEY}";
                var k = Registry.GetValue(key, "UninstallString", null);
                if (k == null)
                {
                    continue;
                }
                return System.IO.Path.GetDirectoryName(k.ToString());
            }

            Logger.Instance.LogMessage(TracingLevel.FATAL, $"{this.GetType()} Voicemeeter not found");
            return null;
        }
        public VmClient()
        {
           System.Threading.Thread.Sleep(15000);
            //Find Voicemeeter dir.
            var vmDir = GetVoicemeeterDir();
            if (vmDir != null)
            {
                VoiceMeeterRemote.LoadDll(System.IO.Path.Combine(vmDir, Constants.VOICEMEETER_REMOTE_DLL));
                LoginResponse = VoiceMeeterRemote.Login();
            }
        }
        public float GetParam(string n)
        {
            float output = -1;
            VoiceMeeterRemote.GetParameter(n, ref output);
            return output;
        }

        public string GetParamString( string n)
        {
            //string output = "";
            StringBuilder output = new StringBuilder(512);
            VoiceMeeterRemote.GetParameter(n, output);

            return output.ToString();
        }

        public void SetParam(string n, float v)
        {
            VoiceMeeterRemote.SetParameter(n, v);
        }

        public void SetParam(string n, string v)
        {
            VoiceMeeterRemote.SetParameter(n, v);
        }

        public void SetParameters(string parameters)
        {
            VoiceMeeterRemote.SetParameters(parameters);
        }

        public void SetMacroStatus(int buttonId, float value)
        {
            VoiceMeeterRemote.SetMacroStatus(buttonId, value, 0);
        }

        public float GetMacroStatus(int buttonId)
        {
            float value = 0;
            VoiceMeeterRemote.GetMacroStatus(buttonId, ref value, 0);
            return value;
        }

        public int Poll()
        {
            return VoiceMeeterRemote.IsParametersDirty() + VoiceMeeterRemote.IsMacrosDirty();
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Console.WriteLine($"VmClient Disposing {disposing}");
                _onClose?.Invoke();
                VoiceMeeterRemote.Logout();
            }
            disposed = true;
        }
        ~VmClient() { Dispose(false); }
        public void OnClose(Action a)
        {
            _onClose = a;
        }
    }
}
