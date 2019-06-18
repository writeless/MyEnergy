namespace EdpConsole.Core
{
    public enum RegistersAddressMessage : byte
    {
        Clock = 0x0001,

        DemandManagementStatus = 0x0013,
        DemandManagementPeriodDefinition = 0x0014,
        ResidualPowerThreshold = 0x0015,

        //tensao, medida volts
        InstantaneousVoltageL1 = 0x006C,

        //medida amperes
        InstantaneousCurrentL1 = 0x006D,

        //medido em watts
        InstantaneousActivePowerPositiveSumAllPhases = 0x0079,

        InstantaneousActivePowerNegativeSumAllPhases = 0x007A,
        InstantaneousPowerFactor = 0x007B,

        //quantidades de ciclos em 10s, medido em hertz
        InstantaneousFrequency = 0x007F,

        ConfiguredMeasurements = 0x0080,
        CapturePeriod = 0x0081,
        EntriesInUse = 0x0082,
        ProfileEntries = 0x0083,
    }
}