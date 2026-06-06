namespace MetanoBR.Structs;

public struct GeoCoordinate
{
    public double Latitude { get; }
    public double Longitude { get; }
    public double Altitude { get; }

    public GeoCoordinate(double latitude, double longitude, double altitude = 0)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
    }

    public override string ToString() =>
        $"({Latitude:F4}°, {Longitude:F4}°, Alt: {Altitude:F1}m)";
}

public struct MethaneReading
{
    public double ValuePpm { get; }
    public DateTime Timestamp { get; }
    public string SensorId { get; }

    public MethaneReading(string sensorId, double valuePpm, DateTime timestamp)
    {
        SensorId = sensorId;
        ValuePpm = valuePpm;
        Timestamp = timestamp;
    }

    public override string ToString() =>
        $"[{Timestamp:dd/MM/yyyy HH:mm:ss}] Sensor {SensorId}: {ValuePpm:F2} ppm";
}
