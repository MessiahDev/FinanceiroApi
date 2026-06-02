using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FinanceiroApi.CrossCutting.Logging;

public static class SerilogConfiguration
{
    public static void Configure(HostBuilderContext context, LoggerConfiguration config)
    {
        var env = context.HostingEnvironment;

        config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                env.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        if (!env.IsDevelopment())
        {
            // Em produção adicione Seq, Elasticsearch, etc.
            // config.WriteTo.Seq("http://seq:5341");
        }
    }
}
