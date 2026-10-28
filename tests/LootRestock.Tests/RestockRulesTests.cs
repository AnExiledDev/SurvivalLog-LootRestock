using LootRestock.Core;
using Xunit;

namespace LootRestock.Tests;

public class DayRangeTests
{
    [Theory]
    [InlineData("3-5", 3, 5)]
    [InlineData(" 4 ", 4, 4)]
    [InlineData("6 - 9", 6, 9)]
    [InlineData("1-365", 1, 365)]
    public void Reads_a_range_or_a_single_day(string text, int min, int max)
    {
        Assert.True(DayRange.TryParse(text, out var range));
        Assert.Equal(new DayRange(min, max), range);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("0-3")]
    [InlineData("5-3")]
    [InlineData("3-400")]
    [InlineData("-2")]
    [InlineData("3-4-5")]
    [InlineData("soon")]
    public void Refuses_what_is_not_a_usable_range(string? text)
    {
        Assert.False(DayRange.TryParse(text, out _));
    }

    [Fact]
    public void Writes_back_the_way_it_reads()
    {
        Assert.Equal("3-5", new DayRange(3, 5).ToString());
        Assert.Equal("4", new DayRange(4, 4).ToString());
    }
}

public class ExploredTests
{
    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, 4, false)]
    [InlineData(9, 5, true)]
    [InlineData(9, 4, false)]
    [InlineData(1, 1, true)]
    public void Half_the_containers_opened_counts_as_explored(int containers, int opened, bool explored)
    {
        Assert.Equal(explored, RestockRules.IsExplored(containers, opened, 0.5));
    }

    [Fact]
    public void A_location_with_no_containers_or_none_opened_is_never_explored()
    {
        Assert.False(RestockRules.IsExplored(0, 0, 0.05));
        Assert.False(RestockRules.IsExplored(10, 0, 0.05));
    }

    [Fact]
    public void A_broken_share_falls_back_to_half()
    {
        Assert.False(RestockRules.IsExplored(10, 1, 0));
        Assert.False(RestockRules.IsExplored(10, 4, double.NaN));
        Assert.True(RestockRules.IsExplored(10, 5, 7));
        Assert.True(RestockRules.IsExplored(10, 3, 0.3));
    }
}

public class RollTests
{
    [Fact]
    public void Stays_inside_the_range_and_uses_all_of_it()
    {
        var range = new DayRange(3, 5);
        var seen = new HashSet<int>();

        for (var day = 0; day < 300; day++)
        {
            var roll = RestockRules.Roll(1002, day, range);

            Assert.InRange(roll, 3, 5);
            seen.Add(roll);
        }

        Assert.Equal(new[] { 3, 4, 5 }, seen.OrderBy(d => d));
    }

    [Fact]
    public void The_same_location_and_start_day_always_roll_the_same()
    {
        Assert.Equal(RestockRules.Roll(1003, 42, new DayRange(1, 100)), RestockRules.Roll(1003, 42, new DayRange(1, 100)));
    }

    [Fact]
    public void Each_cycle_rolls_again()
    {
        var rolls = Enumerable.Range(0, 20).Select(day => RestockRules.Roll(1003, day, new DayRange(1, 100))).Distinct().Count();

        Assert.True(rolls > 1);
    }

    [Fact]
    public void A_single_day_range_always_rolls_that_day()
    {
        Assert.Equal(4, RestockRules.Roll(1000, 17, new DayRange(4, 4)));
    }
}

public class LocationStateTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(250)]
    public void Day_zero_and_later_survive_the_save(int day)
    {
        Assert.Equal(day, LocationState.Decode(LocationState.Encode(day)));
    }

    [Fact]
    public void Unset_is_stored_as_zero()
    {
        Assert.Equal(0, LocationState.Encode(null));
        Assert.Null(LocationState.Decode(0));
    }
}

public class ArriveTests
{
    private const int School = 1002;

    private static readonly RestockSettings Fixed = new(true, 0.5, new Dictionary<int, DayRange> { [School] = new(4, 4) }, new DayRange(9, 9));

    private static Arrival Arrive(int today, int containers, int opened, int? due, int? lastVisit, int map = School) =>
        RestockRules.Arrive(map, new Visit(today, containers, opened, new LocationState(due, lastVisit)), Fixed);

    [Fact]
    public void An_unexplored_location_starts_no_timer()
    {
        var arrival = Arrive(today: 10, containers: 10, opened: 4, due: null, lastVisit: 8);

        Assert.False(arrival.Restock);
        Assert.Null(arrival.DueDay);
        Assert.Equal(new LocationState(null, 10), arrival.Next);
    }

    [Fact]
    public void The_timer_starts_on_the_visit_where_the_location_was_explored()
    {
        // Half opened on day 8, noticed when you come back on day 15: the timer counts from day 8, so it is already due.
        var arrival = Arrive(today: 15, containers: 10, opened: 5, due: null, lastVisit: 8);

        Assert.Equal(8, arrival.TimerStartDay);
        Assert.Equal(12, arrival.DueDay);
        Assert.True(arrival.Restock);
        Assert.Equal(new LocationState(null, 15), arrival.Next);
    }

    [Fact]
    public void Back_before_the_timer_runs_out_keeps_waiting()
    {
        var arrival = Arrive(today: 10, containers: 10, opened: 5, due: null, lastVisit: 9);

        Assert.False(arrival.Restock);
        Assert.Equal(13, arrival.DueDay);
        Assert.Equal(new LocationState(13, 10), arrival.Next);

        var later = Arrive(today: 12, containers: 10, opened: 7, due: 13, lastVisit: 10);

        Assert.False(later.Restock);
        Assert.Null(later.DueDay);
        Assert.Equal(new LocationState(13, 12), later.Next);
    }

    [Fact]
    public void Restocks_on_the_due_day_and_then_waits_to_be_explored_again()
    {
        var arrival = Arrive(today: 13, containers: 10, opened: 6, due: 13, lastVisit: 12);

        Assert.True(arrival.Restock);
        Assert.Equal(new LocationState(null, 13), arrival.Next);
    }

    [Fact]
    public void A_restocked_location_left_alone_never_restocks_again()
    {
        // After a restock the opened containers are marked unopened, so the next visits find few opened.
        var state = new LocationState(null, 13);

        for (var day = 14; day < 60; day += 5)
        {
            var arrival = RestockRules.Arrive(School, new Visit(day, 10, 2, state), Fixed);

            Assert.False(arrival.Restock);
            state = arrival.Next;
        }

        Assert.Null(state.DueDay);
    }

    [Fact]
    public void Nothing_opened_means_nothing_to_restock_even_when_due()
    {
        var arrival = Arrive(today: 20, containers: 10, opened: 0, due: 13, lastVisit: 12);

        Assert.False(arrival.Restock);
        Assert.Equal(new LocationState(13, 20), arrival.Next);
    }

    [Fact]
    public void With_no_visit_on_record_the_timer_counts_from_today()
    {
        // The mod was added to a save where the school is already half opened.
        var arrival = Arrive(today: 30, containers: 10, opened: 8, due: null, lastVisit: null);

        Assert.Equal(30, arrival.TimerStartDay);
        Assert.Equal(34, arrival.DueDay);
        Assert.False(arrival.Restock);
    }

    [Fact]
    public void A_location_without_its_own_range_uses_the_other_range()
    {
        var arrival = Arrive(today: 5, containers: 4, opened: 2, due: null, lastVisit: 5, map: 1099);

        Assert.Equal(14, arrival.DueDay);
    }

    [Fact]
    public void Every_known_location_has_a_usable_default()
    {
        foreach (var location in Locations.Known)
        {
            Assert.True(DayRange.TryParse(location.Default.ToString(), out var parsed));
            Assert.Equal(location.Default, parsed);
        }

        Assert.Equal(6, Locations.Known.Select(l => l.MapId).Distinct().Count());
    }
}
