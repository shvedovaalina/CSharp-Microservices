using Application.Extensions;
using Infrastucture.Extensions;
using Infrastucture.Options;
using Presentation.Extensions;
using Presentation.HostedServices;
using Presentation.Interceptor;
using Presentation.Services;
using Task1.Extensions;
using Task1.Options;
using Task2.Provider;
using Task2.Sources;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var provider = new CustomConfigurationProvider();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.Sources.Add(new CustomConfigurationSource(provider));

builder.Services.AddSingleton(provider);

builder.Services.Configure<ConfigurationClientOptions>(options =>
{
    options.BaseUrl = "http://localhost:8080";
    options.PageSize = 10;
});

builder.Services.AddRefitConfigurationService();

string? connectionString = builder.Configuration.GetConnectionString("ConnectingStrings") ?? builder.Configuration.GetConnectionString("Db");
builder.Services.AddOptions<DatabaseOptions>()
    .Configure(options => options.ConnectionString = connectionString);

builder.Services.AddInfrastucture();
builder.Services.AddMigrations(connectionString);
builder.Services.AddApplication();

builder.Services.AddKafkaExtension(builder.Configuration);

builder.Services.AddHostedService<MigrationHostedService>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ErrorMessageInterceptor>();
});

builder.Services.AddGrpcReflection();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5157, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});
WebApplication app = builder.Build();

app.MapGrpcService<OrderGrpcService>();
app.MapGrpcReflectionService();
await app.RunAsync();