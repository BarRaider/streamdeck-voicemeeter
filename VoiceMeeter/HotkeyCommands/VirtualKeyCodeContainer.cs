using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace HotkeyCommands
{
    public class VirtualKeyCodeContainer
    {
        public VirtualKeyCode KeyCode { get; private set; }
        public bool IsExtended
        {
            get
            {
                return !String.IsNullOrEmpty(ExtendedCommand);
            }
        }

        public string ExtendedCommand { get; private set; }

        public string ExtendedData { get; private set; }

        public VirtualKeyCodeContainer(VirtualKeyCode keyCode, string extendedCommand = null, string extendedData = null)
        {
            KeyCode = keyCode;
            ExtendedCommand = extendedCommand;
            ExtendedData = extendedData;
        }
    }
}
