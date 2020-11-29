using Microsoft.Win32;
using System;

namespace VoiceMeeterWrapper
{
    public class VmClient : IDisposable
    {
        public VbLoginResponse LoginResponse { get; private set; }
        private Action _onClose = null;
        private string GetVoicemeeterDir()
        {
            const string regKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            const string uninstKey = "VB:Voicemeeter {17359A74-1236-5467}";
            var key = $"{regKey}\\{uninstKey}";
            var k = Registry.GetValue(key, "UninstallString", null);
            if (k == null)
            {
                throw new Exception("Voicemeeter not found");
            }
            return System.IO.Path.GetDirectoryName(k.ToString());
        }
        public VmClient()
        {
            //Find Voicemeeter dir.
            var vmDir = GetVoicemeeterDir();
            VoiceMeeterRemote.LoadDll(System.IO.Path.Combine(vmDir, "VoicemeeterRemote.dll"));
            LoginResponse = VoiceMeeterRemote.Login();
        }
        public float GetParam(string n)
        {
            float output = -1;
            VoiceMeeterRemote.GetParameter(n, ref output);
            return output;
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
