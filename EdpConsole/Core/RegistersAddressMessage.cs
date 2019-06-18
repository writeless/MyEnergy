namespace EdpConsole.Core
{
    public enum RegistersAddressMessage : byte
    {
        Clock = 0x01,

        InstantaneousActivePowerSumAllPhases = 0x0079,
        InstantaneousPowerFactor = 0x007B,

        ConfiguredMeasurements = 0x0080,
        CapturePeriod = 0x0081,
        EntriesInUse = 0x0082,
        ProfileEntries = 0x0083,
    }
}