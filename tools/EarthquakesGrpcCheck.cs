#:project ../src/Shared/QuakeNote.Shared.Grpc/QuakeNote.Shared.Grpc.csproj
#:package Grpc.Net.Client@2.71.0

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using QuakeNote.Shared.Grpc.Earthquakes;
using System;

// Использование: dotnet run tools/EarthquakesGrpcCheck.cs <earthquakeUid> <lat> <lon>
// Секрет берётся из переменной окружения SERVICE_SECRET.
var uid = args[0];
var lat = double.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
var lon = double.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
var secret = Environment.GetEnvironmentVariable("SERVICE_SECRET")
    ?? throw new InvalidOperationException("Задайте SERVICE_SECRET.");

using var channel = GrpcChannel.ForAddress("http://localhost:5112");
var client = new EarthquakesGrpc.EarthquakesGrpcClient(channel);
var headers = new Metadata { { "X-Service-Secret", secret } };

var near = await client.FindNearestEarthquakeAsync(new FindNearestEarthquakeRequest
{
    Latitude = lat,
    Longitude = lon,
    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
    WindowMinutes = 30
}, headers);
Console.WriteLine($"FindNearest (+-30 мин): found={near.Found} uid={near.EarthquakeUid} M={near.Magnitude}");

var far = await client.FindNearestEarthquakeAsync(new FindNearestEarthquakeRequest
{
    Latitude = lat,
    Longitude = lon,
    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-3)),
    WindowMinutes = 30
}, headers);
Console.WriteLine($"FindNearest (3 дня назад): found={far.Found} (ожидается False)");

var readings = await client.GetStationReadingsAsync(new GetStationReadingsRequest { EarthquakeUid = uid }, headers);
foreach (var r in readings.Readings)
    Console.WriteLine($"  станция {r.StationUid}: PGA={r.Pga} PGV={r.Pgv} расстояние={r.DistanceToEpicenterKm} км");

try
{
    await client.GetStationReadingsAsync(new GetStationReadingsRequest { EarthquakeUid = Guid.NewGuid().ToString() }, headers);
}
catch (RpcException ex) { Console.WriteLine($"Неизвестное событие: {ex.StatusCode} (ожидается NotFound)"); }

try
{
    await client.GetStationReadingsAsync(new GetStationReadingsRequest { EarthquakeUid = uid });
}
catch (RpcException ex) { Console.WriteLine($"Без секрета: {ex.StatusCode} (ожидается Unauthenticated/PermissionDenied)"); }