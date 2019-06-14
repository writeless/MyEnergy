using System;
using System.Collections.Generic;
using System.Linq;

namespace EdpConsole.Core
{
    public class MeasurementConfiguration
    {
        public List<MeasurementMessage> Types { get; }

        public MeasurementConfiguration(byte[] data)
        {
            Types = new List<MeasurementMessage>();
            foreach (var byteFromData in data)
            {
                Types.Add((MeasurementMessage)byteFromData);
            }
        }

        public bool Contains(MeasurementMessage type)
        {
            return Types.Contains(type);
        }

        public List<MeasurementMessage> AllEnergies()
        {
            return Types.Where(e => e.ToString().Contains("energy", StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }
}