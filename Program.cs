using Azure.Messaging.ServiceBus;
using AzureResponse;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        string cadena = System.Configuration.ConfigurationManager.ConnectionStrings["AzureBusConnection"].ConnectionString;

        services.AddSingleton((p)=>
        {
            return new ServiceBusClient(cadena, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
        });

        services.AddSingleton<IAzBusService, AzBusService>();
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();
