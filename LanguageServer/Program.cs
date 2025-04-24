using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

var server = await LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithLoggerFactory(new LoggerFactory())
                .AddDefaultLoggingProvider()
                .WithHandler<DocumentHandler>()
             );

await server.WaitForExit;
