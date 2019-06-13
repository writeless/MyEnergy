using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    public enum FunctionCode : byte
    {
        None = 0x00,
        ReadRegistersAddress = 0x04,
        ReadLastEntries = 0x44,        ReadEntries = 0x45,
    }
}
