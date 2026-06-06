using MetanoBR.Interfaces;

namespace MetanoBR.Models;

public abstract class Alert
{
    public string Id { get; protected set; }
    public string SensorId { get; protected set; }
    public AlertLevel Level { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public bool IsAcknowledged { get; private set; }
    public string Message { get; protected set; }

    protected Alert(string sensorId, AlertLevel level, string message)
    {
        Id = Guid.NewGuid().ToString("N")[..8].ToUpper();
        SensorId = sensorId;
        Level = level;
        Message = message;
        CreatedAt = DateTime.Now;
        IsAcknowledged = false;
    }

    public void Acknowledge() => IsAcknowledged = true;
    public abstract string GetAlertDescription();

    public TimeSpan GetAge() => DateTime.Now - CreatedAt;
}

public class MethaneLeakAlert : Alert
{
    public double DetectedPpm { get; private set; }
    public double ThresholdPpm { get; private set; }

    public MethaneLeakAlert(string sensorId, double detectedPpm, double thresholdPpm)
        : base(sensorId, ClassifyLevel(detectedPpm),
            $"VAZAMENTO DETECTADO: {detectedPpm:F1} ppm (limiar: {thresholdPpm:F1} ppm)")
    {
        DetectedPpm = detectedPpm;
        ThresholdPpm = thresholdPpm;
    }

    private static AlertLevel ClassifyLevel(double ppm) => ppm switch
    {
        > 5000 => AlertLevel.Critical,
        > 3500 => AlertLevel.High,
        > 2500 => AlertLevel.Medium,
        _ => AlertLevel.Low
    };

    public override string GetAlertDescription() =>
        $"[{Level.ToString().ToUpper()}] Alerta #{Id} | Sensor: {SensorId} | " +
        $"Metano: {DetectedPpm:F1} ppm | {CreatedAt:dd/MM/yyyy HH:mm:ss}";

    public double GetExcessPercentage() =>
        ((DetectedPpm - ThresholdPpm) / ThresholdPpm) * 100;
}

public class SystemAlert : Alert
{
    public string ComponentId { get; private set; }

    public SystemAlert(string componentId, string message)
        : base(componentId, AlertLevel.Low, message)
    {
        ComponentId = componentId;
    }

    public override string GetAlertDescription() =>
        $"[SISTEMA] Alerta #{Id} | Componente: {ComponentId} | {Message} | {CreatedAt:HH:mm:ss}";
}
