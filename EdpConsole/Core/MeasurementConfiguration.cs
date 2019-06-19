using System;
using System.Collections.Generic;
using System.Linq;

namespace EdpConsole.Core
{
    public class MeasurementConfiguration
    {
        public List<MeasurementType> Types { get; }

        public MeasurementConfiguration(byte[] data)
        {
            Types = new List<MeasurementType>();
            foreach (var byteFromData in data.Where(d => d != 0x00 && d != 0xFF))
            {
                Types.Add((MeasurementType)byteFromData);
            }
        }

        public bool Contains(MeasurementType type)
        {
            return Types.Contains(type);
        }

        public List<MeasurementType> AllMeasurement()
        {
            return Types.Where(e => e != MeasurementType.Clock && e != MeasurementType.AMRProfileStatus).ToList();
        }

        public List<MeasurementType> AllEnergies()
        {
            return Types.Where(e => e.ToString().Contains("energy", StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }
}