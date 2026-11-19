using RoomHub.Api.Common;

namespace RoomHub.Api.Domain.Rooms;

public static class RoomErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Room.NameRequired", "Room name is required.");

    public static readonly Error InvalidCapacity =
        Error.Validation("Room.InvalidCapacity", "Capacity must be strictly positive.");

    public static readonly Error NotFound =
        Error.NotFound("Room.NotFound", "The room does not exist.");
}
