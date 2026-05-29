using Sprout.Api.Services;

namespace Sprout.Api.Tests;

public class TestSystemClock : ISystemClock
{
    private DateOnly _today;

    public TestSystemClock(DateOnly? today = null)
    {
        _today = today ?? DateOnly.FromDateTime(DateTime.Now);
    }

    public DateOnly Today() => _today;

    public void SetToday(DateOnly date)
    {
        _today = date;
    }
}
