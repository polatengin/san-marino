using OmniSharp.Extensions.LanguageServer.Server;

var server = await LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithLoggerFactory(new LoggerFactory())
                .WithServices(services =>
                {
                  services.AddScoped<DocumentHandler>();
                })
                .WithHandler<DocumentHandler>()
             );

await server.WaitForExit;
