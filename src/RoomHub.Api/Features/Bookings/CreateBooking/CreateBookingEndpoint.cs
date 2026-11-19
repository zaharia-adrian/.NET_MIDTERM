using RoomHub.Api.Common;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public static class CreateBookingEndpoint
{
    public static IEndpointRouteBuilder MapCreateBooking(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rooms/{roomId:guid}/bookings", async (
                Guid roomId,
                CreateBookingRequest request,
                ICommandHandler<CreateBookingCommand, CreateBookingResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateBookingCommand(
                    roomId,
                    request.BookedBy,
                    request.StartUtc.AsUtc(),
                    request.EndUtc.AsUtc(),
                    request.Attendees);

                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/bookings/{result.Value.Id}", result.Value)
                    : result.ToProblem();
            })
            .WithTags("Bookings");

        return app;
    }
}
