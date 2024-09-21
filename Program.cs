using System.Globalization;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "roll-dice";

Console.Out.WriteLine("Otlp:Endpoint : "  + builder.Configuration.GetValue("Otlp:Endpoint", defaultValue: "http://localhost:4317")!);

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddConsoleExporter()
        .AddOtlpExporter();
});
builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddConsoleExporter()
          .AddOtlpExporter(
            options =>
            {
                //options.Endpoint = new Uri("http://localhost:4317/v1/traces");
                options.Endpoint = new Uri(builder.Configuration.GetValue("Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
                options.Protocol = OtlpExportProtocol.Grpc;
            }
          ))
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddConsoleExporter()
          .AddOtlpExporter());


// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddOpenTelemetry()
//     .WithTracing(tracing => tracing
//         // The rest of your setup code goes here
//         .AddOtlpExporter(options =>
//         {
//             options.Endpoint = new Uri("http://localhost:4317/v1/traces");
//             options.Protocol = OtlpExportProtocol.HttpProtobuf;
//         }))
//     .WithMetrics(metrics => metrics
//         // The rest of your setup code goes here
//         .AddOtlpExporter(options =>
//         {
//             options.Endpoint = new Uri("http://localhost:4317/v1/metrics");
//             options.Protocol = OtlpExportProtocol.HttpProtobuf;
//         }));

// builder.Logging.AddOpenTelemetry(logging => {
//     // The rest of your setup code goes here
//     logging.AddOtlpExporter(options =>
//     {
//         options.Endpoint = new Uri("http://localhost:4317/v1/logs");
//         options.Protocol = OtlpExportProtocol.HttpProtobuf;
//     });
// });

// var tracerProvider = Sdk.CreateTracerProviderBuilder()
//     // Other setup code, like setting a resource goes here too
//     .AddOtlpExporter(options =>
//     {
//         options.Endpoint = new Uri("http://localhost:4317/v1/traces");
//         options.Protocol = OtlpExportProtocol.Grpc;
//     })
//     .Build();

// var meterProvider = Sdk.CreateMeterProviderBuilder()
//     // Other setup code, like setting a resource goes here too
//     .AddOtlpExporter(options =>
//     {
//         options.Endpoint = new Uri("http://localhost:4317/v1/metrics");
//         options.Protocol = OtlpExportProtocol.Grpc;
//     })
//     .Build();

// var loggerFactory = LoggerFactory.Create(builder =>
// {
//     builder.AddOpenTelemetry(logging =>
//     {
//         logging.AddOtlpExporter(options =>
//         {
//             options.Endpoint = new Uri("http://localhost:4317/v1/logs");
//             options.Protocol = OtlpExportProtocol.Grpc;
//         });
//     });
// });

var app = builder.Build();

var logger = app.Logger;

int RollDice()
{
    return Random.Shared.Next(1, 7);
}

string HandleRollDice(string? player)
{
    var result = RollDice();

    if (string.IsNullOrEmpty(player))
    {
        logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
    }
    else
    {
        logger.LogInformation("{player} is rolling the dice: {result}", player, result);
    }

    return result.ToString(CultureInfo.InvariantCulture);
}

app.MapGet("/rolldice/{player?}", HandleRollDice);

app.Run();
