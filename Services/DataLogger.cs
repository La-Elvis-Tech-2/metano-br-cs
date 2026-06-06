using MetanoBR.Interfaces;
using MetanoBR.Structs;

namespace MetanoBR.Services;

public class InMemoryDataLogger : IDataLogger
{
    private readonly List<MethaneReading> _readings = new();
    private readonly object _lock = new();

    public void Log(MethaneReading reading)
    {
        lock (_lock)
        {
            _readings.Add(reading);
        }
    }

    public IEnumerable<MethaneReading> GetHistory(DateTime from, DateTime to)
    {
        lock (_lock)
        {
            return _readings
                .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .ToList();
        }
    }

    public IEnumerable<MethaneReading> GetHistory(string sensorId)
    {
        lock (_lock)
        {
            return _readings
                .Where(r => r.SensorId == sensorId)
                .OrderBy(r => r.Timestamp)
                .ToList();
        }
    }

    public double GetAverage(string sensorId)
    {
        var readings = GetHistory(sensorId).ToList();
        return readings.Count == 0 ? 0 : readings.Average(r => r.ValuePpm);
    }

    public double GetPeak(string sensorId)
    {
        var readings = GetHistory(sensorId).ToList();
        return readings.Count == 0 ? 0 : readings.Max(r => r.ValuePpm);
    }

    public int TotalReadings => _readings.Count;
}
