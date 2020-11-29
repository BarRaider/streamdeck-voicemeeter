using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceMeeterWrapper;
using System.Timers;

namespace VoiceMeeter
{
    internal enum TitleTypeEnum
    {
        VMLive = 0,
        None = 1
    }

    public sealed class VMManager
    {
        #region Private members
        private static VMManager instance = null;
        private static readonly object objLock = new object();
        readonly VmClient client;
        private readonly Timer tmrCheckDirty;

        #endregion

        #region Public Properties

        public bool IsConnected { get; private set; }

        #endregion

        VMManager()
        {
            client = new VmClient();
            IsConnected = true;
            tmrCheckDirty = new Timer();
            tmrCheckDirty.Elapsed += TmrCheckDirty_Elapsed;
            tmrCheckDirty.Interval = 100;
            tmrCheckDirty.Start();
        }

        private void TmrCheckDirty_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsConnected = (client.Poll() >= 0);
        }

        public static VMManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new VMManager();
                    }
                    return instance;
                }
            }
        }

        public bool GetParamBool(string paramName)
        {
            return Convert.ToBoolean(client.GetParam(paramName));
        }

        public string GetParam(string paramName)
        {
            return client.GetParam(paramName).ToString("0.##");
        }

        public void SetParam(string paramName, float value)
        {
            client.SetParam(paramName, value);
        }

        public void SetParam(string paramName, string value)
        {
            client.SetParam(paramName, value);
        }

        public void SetParameters(string parameters)
        {
            client.SetParameters(parameters);
        }

        public bool GetMacroStatus(int buttonId)
        {
            return client.GetMacroStatus(buttonId) > 0;
        }

        public void SetMacroStatus(int buttonId, bool isEnabled)
        {
            client.SetMacroStatus(buttonId, isEnabled ? 1f : 0f);
        }
    }
}
