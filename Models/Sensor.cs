using MetanoBR.Interfaces;
using MetanoBR.Structs;
using MetanoBR.Exceptions;

namespace MetanoBR.Models;

public abstract class Sensor : IMonitorable, IAlertEmitter
{
    private static readonly Random _rng = new();
    public string Id { get; protected set; }
    public string Name { get; protected set; }
    public GeoCoordinate Location { get; protected set; }
    public bool IsActive { get; private set; }
    public double AlertThreshold { get; set; } = 1800.0;
    public DateTime InstalledAt { get; protected set; }

    public event Action<AlertEventArgs>? AlertTriggered;

    protected Sensor(string id, string name, GeoCoordinate location)
    {
        Id = id;
        Name = name;
        Location = location;
        InstalledAt = DateTime.Now;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public abstract MethaneReading GetReading();

    public void CheckThreshold(MethaneReading reading)
    {
        if (reading.ValuePpm > AlertThreshold)
        {
            var level = reading.ValuePpm switch
            {
                > 5000 => AlertLevel.Critical,
                > 3500 => AlertLevel.High,
                > 2500 => AlertLevel.Medium,
                _ => AlertLevel.Low
            };

            AlertTriggered?.Invoke(new AlertEventArgs
            {
                SensorId = reading.SensorId,
                Value = reading.ValuePpm,
                Threshold = AlertThreshold,
                OccurredAt = reading.Timestamp,
                Level = level
            });
        }
    }

    protected double SimulateReading(double baseValue, double variance)
    {
        return baseValue + (_rng.NextDouble() * 2 - 1) * variance;
    }

    public override string ToString() =>
        $"[{GetType().Name}] {Id} - {Name} @ {Location}";
}

public class GroundSensor : Sensor
{
    private readonly double _basePpm;
    private readonly double _variance;

    public GroundSensor(string id, string name, GeoCoordinate location, double basePpm = 1850, double variance = 400)
        : base(id, name, location)
    {
        _basePpm = basePpm;
        _variance = variance;
    }

    public override MethaneReading GetReading()
    {
        if (!IsActive)
            throw new SensorException(Id, $"Sensor {Id} está inativo.");

        double value = Math.Max(0, SimulateReading(_basePpm, _variance));
        return new MethaneReading(Id, value, DateTime.Now);
    }
}

public class AirborneOrbitalSensor : Sensor
{
    public double OrbitAltitudeKm { get; private set; }
    private readonly double _basePpm;

    public AirborneOrbitalSensor(string id, string name, GeoCoordinate location, double orbitAltitudeKm, double basePpm = 1950)
        : base(id, name, location)
    {
        OrbitAltitudeKm = orbitAltitudeKm;
        _basePpm = basePpm;
    }

    public override MethaneReading GetReading()
    {
        if (!IsActive)
            throw new SensorException(Id, $"Sensor orbital {Id} está inativo.");

        double value = Math.Max(0, SimulateReading(_basePpm, 600));
        return new MethaneReading(Id, value, DateTime.Now);
    }
}

public partial class IoTNetworkSensor : Sensor
{
    public string NetworkProtocol { get; private set; }
    public int SignalStrengthDbm { get; private set; }

    public IoTNetworkSensor(string id, string name, GeoCoordinate location, string protocol = "LoRaWAN")
        : base(id, name, location)
    {
        NetworkProtocol = protocol;
        SignalStrengthDbm = -80;
    }

    public override MethaneReading GetReading()
    {
        if (!IsActive)
            throw new SensorException(Id, $"Sensor IoT {Id} está inativo.");

        double value = Math.Max(0, SimulateReading(1700, 500));
        return new MethaneReading(Id, value, DateTime.Now);
    }
}

public partial class IoTNetworkSensor
{
    public void UpdateSignalStrength(int dbm) => SignalStrengthDbm = dbm;

    public string GetNetworkStatus() =>
        SignalStrengthDbm > -90 ? "Conectado" : "Sinal fraco";
}
