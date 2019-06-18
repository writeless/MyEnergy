using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdpConsole.Core
{
    public class Measurement
    {
        public long Id { get; }

        public DateTime Clock { get; }

        public Dictionary<MeasurementType, uint> Values { get; }

        public Measurement(MeasurementConfiguration config, byte[] data)
        {
            Clock = data.ToDateTime();
            Id = Convert.ToInt64(Clock.ToString("yyyyMMddHHmmss"));

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
                values += $" | {key.ToString().Substring(0, 1)}: {Values[key]}";
            }

            return $"{Clock.ToString("dd/MM/yyyy HH:mm:ss")}" + values;
        }
    }

}
