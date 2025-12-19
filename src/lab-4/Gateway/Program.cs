#pragma warning disable CA1506
using Gateway.Middleware;
using Gateway.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Text.Json.Serialization.Metadata;
using Task1.Extensions;
using Task2.Provider;
using Task2.Sources;
using PresentationOrderGrpc = Presentation.OrderService;
using ProcessingOrdersGrpc = Orders.ProcessingService.Contracts.OrderService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var provider = new CustomConfigurationProvider();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.Sources.Add(new CustomConfigurationSource(provider));

builder.Services.AddSingleton(provider);

builder.Services.AddRefitConfigurationService();

builder.Services.Configure<GrpcServiceOptions>(
    builder.Configuration.GetSection("GrpcService"));

builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessingService"));

builder.Services.AddGrpcClient<PresentationOrderGrpc.OrderServiceClient>("PreentationOrderService", (sp, options) =>
{
    GrpcServiceOptions grpcOptions = sp.GetRequiredService<IOptions<GrpcServiceOptions>>().Value;
    options.Address = new Uri(grpcOptions.Address);
});

builder.Services.AddGrpcClient<ProcessingOrdersGrpc.OrderServiceClient>("ProcessingOrdersService", (sp, options) =>
{
    OrderProcessingOptions grpcOptions = sp.GetRequiredService<IOptions<OrderProcessingOptions>>().Value;
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
            Title = "API",
            Description = "lab3 ALINA",
        });
    options.UseOneOfForPolymorphism();
});

WebApplication app = builder.Build();
app.UseMiddleware<ExceptionFormattingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
await app.RunAsync();
#pragma warning restore CA1506