using System;
using System.Runtime.InteropServices;
using System.Text;
using VoiceMeeter;

namespace VoiceMeeterWrapper
{
    public static class VoiceMeeterRemote
    {
        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_Login")]
        public static extern VbLoginResponse Login();
        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_Logout")]
        public static extern VbLoginResponse Logout();

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_SetParameterFloat")]
        public static extern int SetParameter(string szParamName, float value);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_SetParameterStringA")]
        public static extern int SetParameter(string szParamName, string value);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_GetParameterFloat")]
        public static extern int GetParameter(string szParamName, ref float value);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_GetParameterStringA")]
        public static extern int GetParameter(string szParamName, StringBuilder value);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_IsParametersDirty")]
        public static extern int IsParametersDirty();

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_MacroButton_IsDirty")]
        public static extern int IsMacrosDirty();

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_GetVoicemeeterVersion")]
        public static extern int GetVoicemeeterVersion(ref long value);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_SetParameters")]
        public static extern int SetParameters(string szParameters);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_MacroButton_GetStatus")]
        public static extern int GetMacroStatus(int buttonId, ref float value, int bitmode);

        [DllImport(Constants.VOICEMEETER_REMOTE_DLL, EntryPoint = "VBVMR_MacroButton_SetStatus")]
        public static extern int SetMacroStatus(int buttonId, float value, int bitmode);

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
