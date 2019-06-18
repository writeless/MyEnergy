using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdpConsole.Core
{
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
