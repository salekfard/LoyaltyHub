using FluentValidation;
using LoyaltyHub.Loyalty.Application;
using LoyaltyHub.Loyalty.Application.Models;
using LoyaltyHub.Loyalty.Api.Services;
using LoyaltyHub.Loyalty.Infrastructure;
using LoyaltyHub.Loyalty.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    var grpcPort = builder.Configuration.GetValue<int?>("Kestrel:GrpcHttp2Port");
    if (grpcPort is > 0)
    {
        options.ListenAnyIP(grpcPort.Value, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    }
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AppSettings>>().Value);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "LoyaltyHub Loyalty API",
        Version = "v1",
        Description = "Loyalty score calculation service (REST and gRPC)."
    });
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

var appSettings = app.Services.GetRequiredService<AppSettings>();

if (appSettings.IsSwaggerActivated)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = feature?.Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors
                .Select(error => new { error.PropertyName, error.ErrorMessage })
                .ToArray();

            await context.Response.WriteAsJsonAsync(new
            {
                title = "Validation failed",
                status = 400,
                errors
            });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            title = "An unexpected error occurred.",
            status = 500
        });
    });
});

var initializer = app.Services.GetRequiredService<LoyaltyDbInitializer>();
await initializer.InitialiseAsync(seedSampleData: appSettings.IsSeedDataActivated);

app.MapControllers();
app.MapGrpcService<LoyaltyScoreGrpcService>();

if (appSettings.IsGrpcReflectionActivated)
{
    app.MapGrpcReflectionService();
}

app.Run();

public partial class Program;
