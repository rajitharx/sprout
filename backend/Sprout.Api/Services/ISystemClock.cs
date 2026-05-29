namespace Sprout.Api.Services;

public interface ISystemClock
{
    DateOnly Today();
}

public class SystemClock : ISystemClock
{
    public DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
}
