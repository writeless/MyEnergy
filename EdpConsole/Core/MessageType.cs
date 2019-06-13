using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    public enum MessageType
    {
        None,
        ActiveCoreFirmwareId,
        LoadProfileConfiguredMeasurements,
        Clock,
        AMRProfileStatus,
        ActiveEnergyPositiveA,
        ActiveEnergyNegativeA,
        ReactiveEnergyPositiveRi,
        ReactiveEnergyPositiveRc,
        ReactiveEnergyNegativeRi,
        ReactiveEnergyNegativeRc,
        ActiveEnergyPositiveAInc,
        ActiveEnergyNegativeAInc,
        ReactiveEnergyPositiveRiInc,
        ReactiveEnergyPositiveRcInc,
        ReactiveEnergyNegativeRiInc,
        ReactiveEnergyNegativeRcInc,
        LastAveragePowerFactor,
        LastAverageVoltageL1,
        LastAverageVoltageL2,
        LastAverageVoltageL3,
        LastAverageAnyPhaseVoltage,
    }
}
