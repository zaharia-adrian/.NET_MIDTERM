namespace RoomHub.Api.Common;

public static class DateTimeExtensions
{
    // The contract says all dates are UTC. System.Text.Json gives:
    //   "...Z"       -> Kind = Utc         (keep)
    //   "...+02:00"  -> Kind = Local       (convert to UTC)
    //   "..." no zone -> Kind = Unspecified (treat as UTC, NOT as the server's local time)
    public static DateTime AsUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
