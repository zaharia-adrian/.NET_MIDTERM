using RoomHub.Api.Common;

namespace RoomHub.Api.Features.Bookings.GetBooking;

public static class GetBookingEndpoint
{
    public static IEndpointRouteBuilder MapGetBooking(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/{id:guid}", async (
                Guid id,
                IQueryHandler<GetBookingQuery, GetBookingResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new GetBookingQuery(id), cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithTags("Bookings");

        return app;
    }
}
