using MetanoBR.Models;
using MetanoBR.Services;
using MetanoBR.Structs;
using MetanoBR.Exceptions;
using MetanoBR.Interfaces;

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintBanner();

var logger = new InMemoryDataLogger();
var monitoring = new MonitoringService(logger);

SeedInfrastructure(monitoring);

bool running = true;
while (running)
{
    PrintMenu();
    string? input = Console.ReadLine()?.Trim();
    Console.WriteLine();

    try
    {
        running = HandleOption(input, monitoring, logger);
    }
    catch (ThresholdExceededException ex)
    {
        PrintError($"Limiar excedido: {ex.DetectedValue:F1} ppm > {ex.Threshold:F1} ppm");
    }
    catch (SensorException ex)
    {
        PrintError($"Erro de sensor [{ex.SensorId}]: {ex.Message}");
    }
    catch (SatelliteConnectionException ex)
    {
        PrintError($"Erro de satélite [{ex.SatelliteId}]: {ex.Message}");
    }
    catch (Exception ex)
    {
        PrintError($"Erro inesperado: {ex.Message}");
    }
}

static void SeedInfrastructure(MonitoringService monitoring)
{
    var s1 = new GroundSensor("GS-001", "Sensor Amazônia Norte", new GeoCoordinate(-2.5, -60.0), 2100, 600);
    var s2 = new GroundSensor("GS-002", "Sensor Pantanal Central", new GeoCoordinate(-17.0, -57.5), 1600, 300);
    var s3 = new AirborneOrbitalSensor("AO-001", "Sensor Orbital LEO-A", new GeoCoordinate(-5.0, -45.0), 550, 1950);
    var s4 = new IoTNetworkSensor("IOT-001", "Sensor IoT Cerrado", new GeoCoordinate(-15.0, -47.0), "LoRaWAN");

    s1.AlertThreshold = 1800;
    s2.AlertThreshold = 1800;
    s3.AlertThreshold = 2200;
    s4.AlertThreshold = 1800;

    s1.Activate();
    s2.Activate();
    s3.Activate();
    s4.Activate();

    monitoring.RegisterSensor(s1);
    monitoring.RegisterSensor(s2);
    monitoring.RegisterSensor(s3);
    monitoring.RegisterSensor(s4);

    var sat1 = new LeoBandSatellite("SAT-LEO-01", "MetanoSat-1", 550);
    var sat2 = new GeostationarySatellite("SAT-GEO-01", "MetanoGEO-BR", -45.0);

    monitoring.RegisterSatellite(sat1);
    monitoring.RegisterSatellite(sat2);

    Console.WriteLine("Infraestrutura iniciada: 4 sensores, 2 satélites.\n");
}

static bool HandleOption(string? option, MonitoringService monitoring, InMemoryDataLogger logger)
{
    switch (option)
    {
        case "1": ShowSensors(monitoring); break;
        case "2": ShowSatellites(monitoring); break;
        case "3": CollectReadings(monitoring); break;
        case "4": ShowAlerts(monitoring); break;
        case "5": AcknowledgeAlert(monitoring); break;
        case "6": ShowHistory(logger); break;
        case "7": ShowStatistics(monitoring, logger); break;
        case "8": SimulateCriticalLeak(monitoring); break;
        case "0":
            Console.WriteLine("Encerrando MetanoBR.");
            return false;
        default:
            Console.WriteLine("Opção inválida.");
            break;
    }
    return true;
}

static void ShowSensors(MonitoringService monitoring)
{
    Console.WriteLine("Sensores registrados:");
    if (monitoring.Sensors.Count == 0)
    {
        Console.WriteLine("Nenhum sensor registrado.");
        return;
    }
    foreach (var sensor in monitoring.Sensors)
    {
        string status = sensor.IsActive ? "ativo" : "inativo";
        Console.WriteLine($"{sensor.Id} | {sensor.Name} | {status} | limiar {sensor.AlertThreshold} ppm | instalado em {sensor.InstalledAt:dd/MM/yyyy}");
    }
}

static void ShowSatellites(MonitoringService monitoring)
{
    Console.WriteLine("Satélites:");
    foreach (var sat in monitoring.Satellites)
    {
        Console.WriteLine($"{sat.Id} | {sat.Name} | {sat.GetStatus()}");
        Console.WriteLine($"   tempo operacional: {sat.GetOperationalTime().Days} dias");
    }
    monitoring.UpdateSatellitePositions();
    Console.WriteLine("Posições atualizadas.");
}

static void CollectReadings(MonitoringService monitoring)
{
    Console.WriteLine("Coleta de leituras:");
    var readings = monitoring.CollectAllReadings();
    foreach (var r in readings)
    {
        string level = r.ValuePpm > 2500 ? "ALTO" : r.ValuePpm > 1800 ? "MEDIO" : "NORMAL";
        Console.WriteLine($"{r} - {level}");
    }
    Console.WriteLine($"Total: {readings.Count} leituras as {DateTime.Now:HH:mm:ss}");
}

static void ShowAlerts(MonitoringService monitoring)
{
    Console.WriteLine("Alertas ativos:");
    var alerts = monitoring.GetUnacknowledgedAlerts().ToList();
    if (alerts.Count == 0)
    {
        Console.WriteLine("Nenhum alerta ativo.");
        return;
    }
    foreach (var alert in alerts)
        Console.WriteLine(alert.GetAlertDescription());

    Console.WriteLine($"Total nao confirmados: {alerts.Count}");
}

static void AcknowledgeAlert(MonitoringService monitoring)
{
    Console.WriteLine("Confirmar alerta:");
    var alerts = monitoring.GetUnacknowledgedAlerts().ToList();
    if (alerts.Count == 0)
    {
        Console.WriteLine("Nenhum alerta pendente.");
        return;
    }
    foreach (var a in alerts)
        Console.WriteLine($"ID {a.Id}: {a.Message}");

    Console.Write("Digite o ID do alerta: ");
    string? id = Console.ReadLine()?.Trim().ToUpper();
    if (string.IsNullOrEmpty(id))
    {
        Console.WriteLine("ID inválido.");
        return;
    }

    monitoring.AcknowledgeAlert(id);
    Console.WriteLine($"Alerta {id} confirmado.");
}

static void ShowHistory(InMemoryDataLogger logger)
{
    Console.WriteLine("Histórico de leituras:");
    Console.Write("Sensor ID (ou ENTER para todos): ");
    string? sensorId = Console.ReadLine()?.Trim();

    IEnumerable<MethaneReading> history;
    if (string.IsNullOrEmpty(sensorId))
        history = logger.GetHistory(DateTime.Today, DateTime.Now);
    else
        history = logger.GetHistory(sensorId);

    var list = history.ToList();
    if (list.Count == 0)
    {
        Console.WriteLine("Nenhum dado encontrado.");
        return;
    }

    foreach (var r in list.TakeLast(10))
        Console.WriteLine(r);

    if (list.Count > 10)
        Console.WriteLine($"... e mais {list.Count - 10} registro(s) anteriores.");

    Console.WriteLine($"Total: {list.Count} leituras");
}

static void ShowStatistics(MonitoringService monitoring, InMemoryDataLogger logger)
{
    Console.WriteLine("Estatísticas:");
    Console.WriteLine($"Data/hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
    Console.WriteLine($"Total de leituras: {logger.TotalReadings}");
    Console.WriteLine($"Sensores ativos: {monitoring.Sensors.Count(s => s.IsActive)}/{monitoring.Sensors.Count}");
    Console.WriteLine($"Satélites: {monitoring.Satellites.Count}");
    Console.WriteLine($"Total de alertas: {monitoring.Alerts.Count}");
    Console.WriteLine($"Alertas pendentes: {monitoring.GetUnacknowledgedAlerts().Count()}");
    Console.WriteLine();

    foreach (var sensor in monitoring.Sensors)
    {
        double avg = logger.GetAverage(sensor.Id);
        double peak = logger.GetPeak(sensor.Id);
        if (avg == 0) continue;

        Console.WriteLine($"{sensor.Id} | media {avg:F1} ppm | pico {peak:F1} ppm");
    }
}

static void SimulateCriticalLeak(MonitoringService monitoring)
{
    Console.WriteLine("Simulação de vazamento crítico:");

    var sensor = monitoring.Sensors.FirstOrDefault(s => s.IsActive);
    if (sensor == null)
    {
        Console.WriteLine("Nenhum sensor ativo disponível.");
        return;
    }

    double value = 6200.0;
    Console.WriteLine($"Sensor: {sensor.Id} - {sensor.Name}");
    Console.WriteLine($"Valor: {value:F1} ppm (crítico)");
    Console.WriteLine($"Horário: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

    var alert = new MethaneLeakAlert(sensor.Id, value, sensor.AlertThreshold);
    Console.WriteLine($"Alerta: {alert.GetAlertDescription()}");
    Console.WriteLine($"Excesso sobre o limiar: {alert.GetExcessPercentage():F1}%");
}

static void PrintBanner()
{
    Console.WriteLine("MetanoBR - Monitoramento de vazamentos de metano (CH4)");
    Console.WriteLine("FIAP Global Solution 2025");
    Console.WriteLine();
}

static void PrintMenu()
{
    Console.WriteLine("Menu");
    Console.WriteLine("1. Listar sensores");
    Console.WriteLine("2. Status dos satélites");
    Console.WriteLine("3. Coletar leituras");
    Console.WriteLine("4. Ver alertas ativos");
    Console.WriteLine("5. Confirmar alerta");
    Console.WriteLine("6. Histórico de leituras");
    Console.WriteLine("7. Estatísticas");
    Console.WriteLine("8. Simular vazamento crítico");
    Console.WriteLine("0. Sair");
    Console.Write("Opção: ");
}

static void PrintError(string message)
{
    Console.WriteLine($"[ERRO] {message}");
}
