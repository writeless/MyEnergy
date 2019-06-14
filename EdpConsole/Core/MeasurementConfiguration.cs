using EdpConsole.Extensions;
using System;
using System.Collections;
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

    public class Measurement
    {
        public DateTime Clock { get; }

        public Dictionary<MeasurementType, uint> Values { get; }

        public Measurement(MeasurementConfiguration config, byte[] data)
        {
            Clock = data.ToDateTime();

            var measurementData = data.Skip(13).ToArray();
            Values = new Dictionary<MeasurementType, uint>();

            var index = 0;
            foreach (var measurement in config.AllMeasurement())
            {
                var value = measurementData.Skip(index * 2).Take(2).ToArray().ToUInt32();
                Values.Add(measurement, value);
                index++;
            }
        }

        public uint SumByType(List<MeasurementType> types)
        {
            //TODO:
            return (uint)Values.Where(v => types.Contains(v.Key)).Select(v => (decimal)v.Value).Sum();
        }

        public override string ToString()
        {
            var values = "";

            foreach (var key in Values.Keys)
            {
                values += $" | {key.ToString().Substring(0,1)}: {Values[key]}";
            }

            return $"{Clock.ToString("dd/MM/yyyy HH:mm:ss")}" + values;
        }
    }

    public class Measurements : IEnumerable<Measurement>
    {
        private List<Measurement> _measurements;

        public Measurements()
        {
            _measurements = new List<Measurement>();
        }

        public Measurements(MeasurementConfiguration config, int length, byte[] data)
        {
            _measurements = new List<Measurement>();
            var dataLength = data.Length / length;

            for (int i = 0; i < length; i++)
            {
                var measurementData = data.Skip(i * dataLength).Take(dataLength).ToArray();
                var measurement = new Measurement(config, measurementData);
                _measurements.Add(measurement);
            }
        }

        public IEnumerator<Measurement> GetEnumerator()
        {
            return _measurements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _measurements.GetEnumerator();
        }

        public uint SumByType(List<MeasurementType> types)
        {
            //TODO:
            return (uint)_measurements.Select(v => (decimal)v.SumByType(types)).Sum();
        }

        public void AddRange(Measurements measurements)
        {
            _measurements.AddRange(measurements);
        }
    }
}