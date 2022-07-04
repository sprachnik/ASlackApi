using App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration((h,c) =>
    {
        c.AddJsonFile("local.settings.json", optional: true)
          .AddEnvironmentVariables();
    })
    .ConfigureServices(s => 
    {
        Startup.Configure(s);
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();