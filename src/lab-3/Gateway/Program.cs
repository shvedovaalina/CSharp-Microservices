#pragma warning disable CA1506
using Gateway.Middleware;
using Gateway.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Presentation;
using System.Text.Json.Serialization.Metadata;
using Task1.Extensions;
using Task2.Options;
using Task2.Provider;
using Task2.Sources;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var provider = new CustomConfigurationProvider();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.Sources.Add(new CustomConfigurationSource(provider));

builder.Services.AddSingleton(provider);

builder.Services.Configure<ConfigurationUpdateOptions>(options =>
{
    options.UpdateInterval = 10;
});

builder.Services.AddRefitConfigurationService();

builder.Services.Configure<GrpcServiceOptions>(
    builder.Configuration.GetSection("GrpcService"));

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>((sp, options) =>
{
    GrpcServiceOptions grpcOptions = sp.GetRequiredService<IOptions<GrpcServiceOptions>>().Value;
    options.Address = new Uri(grpcOptions.Address);
});

builder.Services.AddTransient<ExceptionFormattingMiddleware>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.TypeInfoResolver =
            new DefaultJsonTypeInfoResolver();
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "1",
            Title = "Gateway API",
            Description = "lab3 ALINA",
        });
    options.UseAllOfToExtendReferenceSchemas();
    options.UseOneOfForPolymorphism();
    options.SelectDiscriminatorNameUsing(_ => "$type");
    options.SelectDiscriminatorValueUsing(subType => subType.Name);
});

WebApplication app = builder.Build();
app.UseMiddleware<ExceptionFormattingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
await app.RunAsync();
#pragma warning restore CA1506