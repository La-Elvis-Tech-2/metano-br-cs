using MetanoBR.Interfaces;
using MetanoBR.Models;
using MetanoBR.Structs;
using MetanoBR.Exceptions;

namespace MetanoBR.Services;

public class MonitoringService
{
    private readonly List<Sensor> _sensors = new();
    private readonly List<Satellite> _satellites = new();
    private readonly List<Alert> _alerts = new();
    private readonly IDataLogger _logger;

    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();
    public IReadOnlyList<Satellite> Satellites => _satellites.AsReadOnly();
    public IReadOnlyList<Alert> Alerts => _alerts.AsReadOnly();

    public MonitoringService(IDataLogger logger)
    {
        _logger = logger;
    }

    public void RegisterSensor(Sensor sensor)
    {
        sensor.AlertTriggered += OnAlertTriggered;
        _sensors.Add(sensor);
    }

    public void RegisterSatellite(Satellite satellite)
    {
        _satellites.Add(satellite);
    }

    private void OnAlertTriggered(AlertEventArgs args)
    {
        var alert = new MethaneLeakAlert(args.SensorId, args.Value, args.Threshold);
        _alerts.Add(alert);
    }

    public MethaneReading CollectReading(string sensorId)
    {
        var sensor = _sensors.FirstOrDefault(s => s.Id == sensorId)
            ?? throw new SensorException(sensorId, $"Sensor {sensorId} não encontrado.");

        var reading = sensor.GetReading();
        _logger.Log(reading);
        sensor.CheckThreshold(reading);
        return reading;
    }

    public List<MethaneReading> CollectAllReadings()
    {
        var results = new List<MethaneReading>();
        foreach (var sensor in _sensors.Where(s => s.IsActive))
        {
            try
            {
                results.Add(CollectReading(sensor.Id));
            }
            catch (SensorException ex)
            {
                Console.WriteLine($"  [ERRO] {ex.Message}");
            }
        }
        return results;
    }

    public void UpdateSatellitePositions()
    {
        foreach (var satellite in _satellites)
        {
            try
            {
                satellite.UpdatePosition();
            }
            catch (SatelliteConnectionException ex)
            {
                Console.WriteLine($"  [ERRO SATÉLITE] {ex.Message}");
                var sysAlert = new SystemAlert(ex.SatelliteId, ex.Message);
                _alerts.Add(sysAlert);
            }
        }
    }

    public IEnumerable<Alert> GetUnacknowledgedAlerts() =>
        _alerts.Where(a => !a.IsAcknowledged).OrderByDescending(a => a.CreatedAt);

    public void AcknowledgeAlert(string alertId)
    {
        var alert = _alerts.FirstOrDefault(a => a.Id == alertId)
            ?? throw new InvalidOperationException($"Alerta {alertId} não encontrado.");
        alert.Acknowledge();
    }
}
