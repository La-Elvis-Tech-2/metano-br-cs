using MetanoBR.Structs;
using MetanoBR.Exceptions;

namespace MetanoBR.Models;

public abstract class Satellite
{
    public string Id { get; protected set; }
    public string Name { get; protected set; }
    public GeoCoordinate CurrentPosition { get; protected set; }
    public bool IsOperational { get; protected set; }
    public DateTime LaunchDate { get; protected set; }

    protected Satellite(string id, string name, GeoCoordinate position, DateTime launchDate)
    {
        Id = id;
        Name = name;
        CurrentPosition = position;
        LaunchDate = launchDate;
        IsOperational = true;
    }

    public abstract void UpdatePosition();
    public abstract string GetStatus();

    public TimeSpan GetOperationalTime() => DateTime.Now - LaunchDate;
}

public class LeoBandSatellite : Satellite
{
    public double OrbitalRadiusKm { get; private set; }
    private double _angle = 0;
    private static readonly Random _rng = new();

    public LeoBandSatellite(string id, string name, double orbitalRadiusKm)
        : base(id, name, new GeoCoordinate(0, 0, orbitalRadiusKm * 1000), new DateTime(2022, 3, 15))
    {
        OrbitalRadiusKm = orbitalRadiusKm;
    }

    public override void UpdatePosition()
    {
        if (!IsOperational)
            throw new SatelliteConnectionException(Id, $"Satélite {Id} não está operacional.");

        _angle = (_angle + 0.5) % 360;
        double lat = Math.Sin(_angle * Math.PI / 180) * 60;
        double lon = (_angle * 2) % 360 - 180;
        CurrentPosition = new GeoCoordinate(lat, lon, OrbitalRadiusKm * 1000);
    }

    public override string GetStatus() =>
        IsOperational
            ? $"OPERACIONAL | Órbita: {OrbitalRadiusKm} km | Posição: {CurrentPosition}"
            : "FALHA";
}

public class GeostationarySatellite : Satellite
{
    public double LongitudeFixed { get; private set; }

    public GeostationarySatellite(string id, string name, double fixedLongitude)
        : base(id, name, new GeoCoordinate(0, fixedLongitude, 35786000), new DateTime(2020, 7, 4))
    {
        LongitudeFixed = fixedLongitude;
    }

    public override void UpdatePosition()
    {
        if (!IsOperational)
            throw new SatelliteConnectionException(Id, $"Satélite geoestacionário {Id} sem comunicação.");
    }

    public override string GetStatus() =>
        $"GEOESTACIONÁRIO | Longitude fixa: {LongitudeFixed}° | Alt: 35.786 km";
}
