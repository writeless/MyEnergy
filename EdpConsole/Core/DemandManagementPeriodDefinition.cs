using EdpConsole.Extensions;
using System;
using System.Linq;

namespace EdpConsole.Core
{
    public class DemandManagementPeriodDefinition
    {
        public DemandManagementStatus Status { get; set; }

        public DateTime StartPeriod { get; set; }

        public DateTime EndPeriod { get; set; }

        public uint DecreasePercentage { get; set; }

        public uint AbsolutePowerValue { get; set; }

        public DemandManagementPeriodDefinition(byte[] data)
        {
            Status = (DemandManagementStatus)data[0];

            if (Status != DemandManagementStatus.NoActivePeriod)
            {
                StartPeriod = data.Skip(1).Take(12).ToArray().ToDateTime();
                EndPeriod = data.Skip(13).Take(12).ToArray().ToDateTime();
                DecreasePercentage = data.Skip(25).Take(1).ToArray()[0];
                AbsolutePowerValue = data.Skip(26).Take(4).ToArray().ToUInt32();
            }
        }

        public override string ToString()
        {
            var result = $"PeriodType: {Status.ToString()}";

            if (Status != DemandManagementStatus.NoActivePeriod)
            {
                result += $"StartPeriod | : {StartPeriod.ToString("dd/MM/yyyy")}"
                        + $"EndPeriod | : {EndPeriod.ToString("dd/MM/yyyy")}"
                        + $"DecreasePercentage | : {DecreasePercentage}"
                        + $"AbsolutePowerValue | : {AbsolutePowerValue}";
            }

            return result;
        }
    }
}