using MetanoBR.Structs;

namespace MetanoBR.Interfaces;

public interface IMonitorable
{
    string Id { get; }
    bool IsActive { get; }
    MethaneReading GetReading();
    void Activate();
    void Deactivate();
}

public interface IAlertEmitter
{
    event Action<AlertEventArgs>? AlertTriggered;
    double AlertThreshold { get; set; }
    void CheckThreshold(MethaneReading reading);
}

public interface IDataLogger
{
    void Log(MethaneReading reading);
    IEnumerable<MethaneReading> GetHistory(DateTime from, DateTime to);
    IEnumerable<MethaneReading> GetHistory(string sensorId);
}

public class AlertEventArgs : EventArgs
{
    public string SensorId { get; init; } = string.Empty;
    public double Value { get; init; }
    public double Threshold { get; init; }
    public DateTime OccurredAt { get; init; }
    public AlertLevel Level { get; init; }
}

public enum AlertLevel { Low, Medium, High, Critical }
