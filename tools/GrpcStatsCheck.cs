#:package Grpc.Net.Client@2.*
#:project ../src/Shared/QuakeNote.Shared.Grpc/QuakeNote.Shared.Grpc.csproj

using Grpc.Core;
using Grpc.Net.Client;
using QuakeNote.Shared.Grpc.Reports;
using System;

// Использование: dotnet run tools/GrpcStatsCheck.cs <earthquake_uid> [secret] [address]
var uid = args.Length > 0 ? args[0] : "11111111-1111-1111-1111-111111111111";
var secret = args.Length > 1 ? args[1] : null;
var address = args.Length > 2 ? args[2] : "http://localhost:5113";

using var channel = GrpcChannel.ForAddress(address);
var client = new ReportsGrpc.ReportsGrpcClient(channel);

var headers = new Metadata();
if (!string.IsNullOrEmpty(secret)) headers.Add("X-Service-Secret", secret);

try
{
    var reply = await client.GetReportsStatsByEarthquakeAsync(
        new ReportsStatsRequest { EarthquakeUid = uid }, headers);
    Console.WriteLine(reply);
}
catch (RpcException ex)
{
    Console.WriteLine($"{ex.StatusCode}: {ex.Status.Detail}");
}