using EventGateway.Data;
using EventGateway.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EventGateway.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
            .WithName("HealthCheck")
            .Produces(StatusCodes.Status200OK);

        app.MapGet("/events/{id}", async Task<Results<Ok<EventResponse>, NotFound<ApiErrorResponse>>> (string id, EventLedgerDbContext db) =>
        {
            var eventEntity = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity is null)
            {
                return TypedResults.NotFound(new ApiErrorResponse
                {
                    Message = "The requested event was not found."
                });
            }

            return TypedResults.Ok(MapToResponse(eventEntity));
        })
        .WithName("GetEventById")
        .Produces<EventResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        app.MapGet("/events", async Task<Ok<List<EventResponse>>> (string? account, EventLedgerDbContext db) =>
        {
            var query = db.Events
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(account))
            {
                query = query.Where(e => e.AccountId == account.Trim());
            }

            var events = await query
                .OrderByDescending(e => e.EventTimestamp)
                .ThenByDescending(e => e.Id)
                .ToListAsync();

            return TypedResults.Ok(events.Select(MapToResponse).ToList());
        })
        .WithName("ListEvents")
        .Produces<List<EventResponse>>(StatusCodes.Status200OK);

        app.MapPost("/events", async Task<Results<Created<EventResponse>, Conflict<DuplicateEventResponse>, BadRequest<ApiErrorResponse>>> (CreateEventRequest request, EventLedgerDbContext db) =>
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);

            if (validationResults.Count > 0)
            {
                var errors = validationResults
                    .SelectMany(v => v.MemberNames.DefaultIfEmpty(string.Empty), (v, name) => new KeyValuePair<string, string[]>(name, new[] { v.ErrorMessage ?? "Validation failed." }))
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.SelectMany(x => x.Value).ToArray());

                return TypedResults.BadRequest(new ApiErrorResponse
                {
                    Message = "The request contains invalid data.",
                    Errors = errors
                });
            }

            var normalizedType = request.Type.Trim().ToUpperInvariant();
            var eventId = request.EventId.Trim();
            var accountId = request.AccountId.Trim();
            var currency = request.Currency.Trim().ToUpperInvariant();

            var existing = await db.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (existing is not null)
            {
                return TypedResults.Conflict(new DuplicateEventResponse
                {
                    Message = "Duplicate event submitted. The original event is returned without altering balances.",
                    Event = MapToResponse(existing)
                });
            }

            var metadataJson = request.Metadata is null || request.Metadata.Count == 0
                ? null
                : JsonSerializer.Serialize(request.Metadata);

            var eventEntity = new EventEntity
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                AccountId = accountId,
                Type = normalizedType,
                Amount = request.Amount,
                Currency = currency,
                EventTimestamp = request.EventTimestamp,
                MetadataJson = metadataJson
            };

            db.Events.Add(eventEntity);

            var balance = await db.AccountBalances
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (balance is null)
            {
                balance = new AccountBalanceEntity
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Balance = 0m
                };

                db.AccountBalances.Add(balance);
            }

            balance.Balance += string.Equals(normalizedType, "CREDIT", StringComparison.OrdinalIgnoreCase)
                ? eventEntity.Amount
                : -eventEntity.Amount;

            await db.SaveChangesAsync();

            var response = MapToResponse(eventEntity);
            return TypedResults.Created($"/events/{eventEntity.EventId}", response);
        })
        .WithName("CreateEvent")
        .Accepts<CreateEventRequest>("application/json")
        .Produces<EventResponse>(StatusCodes.Status201Created)
        .Produces<DuplicateEventResponse>(StatusCodes.Status409Conflict)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static EventResponse MapToResponse(EventEntity entity)
    {
        return new EventResponse
        {
            EventId = entity.EventId,
            AccountId = entity.AccountId,
            Type = entity.Type,
            Amount = entity.Amount,
            Currency = entity.Currency,
            EventTimestamp = entity.EventTimestamp,
            Metadata = string.IsNullOrWhiteSpace(entity.MetadataJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson)
        };
    }
}
