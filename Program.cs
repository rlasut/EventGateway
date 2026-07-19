using EventGateway.Data;
using EventGateway.Endpoints;
using EventGateway.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventLedgerDbContext>(options =>
    options.UseInMemoryDatabase("EventLedgerDb"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logPath = Path.Combine(AppContext.BaseDirectory, "Event.log");

app.Use(async (context, next) =>
{
    var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(traceId))
    {
        traceId = Guid.NewGuid().ToString("N");
    }

    context.TraceIdentifier = traceId;
    context.Request.Headers["X-Trace-Id"] = traceId;
    context.Response.Headers["X-Trace-Id"] = traceId;
    context.Items["TraceId"] = traceId;

    var logEntry = $"{DateTimeOffset.UtcNow:O} | TraceId={traceId} | Method={context.Request.Method} | Path={context.Request.Path}{Environment.NewLine}";
    await File.AppendAllTextAsync(logPath, logEntry);

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EventGateway API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var error = new ApiErrorResponse
        {
            Message = "An unexpected error occurred. Please try again later."
        };

        if (exception is not null)
        {
            error.Errors = new Dictionary<string, string[]>
            {
                ["server"] = new[] { exception.Message }
            };
        }

        await context.Response.WriteAsJsonAsync(error);
    });
});

app.UseHttpsRedirection();

app.MapEventEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventLedgerDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
