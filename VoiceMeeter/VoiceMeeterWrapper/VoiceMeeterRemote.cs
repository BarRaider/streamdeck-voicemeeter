using System;
using System.Runtime.InteropServices;

namespace VoiceMeeterWrapper
{
    public static class VoiceMeeterRemote
    {
        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_Login")]
        public static extern VbLoginResponse Login();
        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_Logout")]
        public static extern VbLoginResponse Logout();

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_SetParameterFloat")]
        public static extern int SetParameter(string szParamName, float value);

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_SetParameterStringA")]
        public static extern int SetParameter(string szParamName, string value);

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_GetParameterFloat")]
        public static extern int GetParameter(string szParamName, ref float value);

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_GetParameterStringA")]
        public static extern int GetParameter(string szParamName, ref string value);

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_IsParametersDirty")]
        public static extern int IsParametersDirty();

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_GetVoicemeeterVersion")]
        public static extern int GetVoicemeeterVersion(ref long value);

        [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_SetParameters")]
        public static extern int SetParameters(string szParameters);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);
        private static IntPtr? _dllHandle;
        public static void LoadDll(string dllPath)
        {
            if (!_dllHandle.HasValue)
            {
                _dllHandle = LoadLibrary(dllPath);
            }
        }
    }
}
