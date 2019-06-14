using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    public enum RegistersAddressMessage : byte
    {
        Clock = 0x01,
        ConfiguredMeasurements = 0x0080,
        CapturePeriod = 0x0081,
        EntriesInUse = 0x0082,
        ProfileEntries = 0x0083,
    }
}
