using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    public enum MeasurementMessage : byte
    {
        Clock = 0x01,
        AMRProfileStatus = 0x02,
        ActiveEnergyPositiveA = 0x03,
        ActiveEnergyNegativeA = 0x04,
        ReactiveEnergyPositiveRi = 0x05,
        ReactiveEnergyPositiveRc = 0x06,
        ReactiveEnergyNegativeRi = 0x07,
        ReactiveEnergyNegativeRc = 0x08,
        ActiveEnergyPositiveAInc = 0x09,
        ActiveEnergyNegativeAInc = 0x0a,
        ReactiveEnergyPositiveRiInc = 0x0b,
        ReactiveEnergyPositiveRcInc = 0x0c,
        ReactiveEnergyNegativeRiInc = 0x0d,
        ReactiveEnergyNegativeRcInc = 0x0e,
        LastAveragePowerFactor = 0x0f,
        LastAverageVoltageL1 = 0x10,
        LastAverageVoltageL2 = 0x11,
        LastAverageVoltageL3 = 0x12,
        LastAverageAnyPhaseVoltage = 0x13,
    }
}
