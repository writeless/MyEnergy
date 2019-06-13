using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    public enum FunctionCode : byte
    {
        ReadLoadProfileConfiguredMeasurements = 0x04,
        ReadLastLoadProfileEntries = 0x44,        ReadLoadProfileEntries = 0x45,
    }
}
