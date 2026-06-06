namespace MetanoBR.Exceptions;

public class SensorException : Exception
{
    public string SensorId { get; }
    public SensorException(string sensorId, string message) : base(message)
    {
        SensorId = sensorId;
    }
}

public class ThresholdExceededException : SensorException
{
    public double DetectedValue { get; }
    public double Threshold { get; }
    public ThresholdExceededException(string sensorId, double value, double threshold)
        : base(sensorId, $"Sensor {sensorId}: valor {value} ppm excede limiar {threshold} ppm")
    {
        DetectedValue = value;
        Threshold = threshold;
    }
}

public class SatelliteConnectionException : Exception
{
    public string SatelliteId { get; }
    public SatelliteConnectionException(string satelliteId, string message)
        : base(message)
    {
        SatelliteId = satelliteId;
    }
}
