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
    public void A_share_that_lands_on_a_whole_count_needs_exactly_that_many()
    {
        // 0.56 * 25 is 14.000000000000002 in floating point; opening 14 of 25 is opening 56%.
        Assert.True(RestockRules.IsExplored(25, 14, 0.56));
        Assert.False(RestockRules.IsExplored(25, 13, 0.56));
        Assert.True(RestockRules.IsExplored(100, 7, 0.07));
        Assert.True(RestockRules.IsExplored(10, 4, 0.31));
        Assert.False(RestockRules.IsExplored(10, 3, 0.31));
    }

    [Fact]
    public void Needs_at_least_one_container_opened()
    {
        Assert.Equal(1, RestockRules.NeededToExplore(1, 0.05));
        Assert.Equal(13, RestockRules.NeededToExplore(25, 0.5));
        Assert.Equal(1, RestockRules.NeededToExplore(0, 0.5));
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

    private static RestockSettings SchoolDays(int min, int max) => Fixed with { Ranges = new Dictionary<int, DayRange> { [School] = new(min, max) } };

    private static Arrival Arrive(int today, int containers, int opened, int? start, int? lastVisit, int map = School, RestockSettings? settings = null) =>
        RestockRules.Arrive(map, new Visit(today, containers, opened, new LocationState(start, lastVisit)), settings ?? Fixed);

    [Fact]
    public void An_unexplored_location_starts_no_timer()
    {
        var arrival = Arrive(today: 10, containers: 10, opened: 4, start: null, lastVisit: 8);

        Assert.False(arrival.Restock);
        Assert.False(arrival.Started);
        Assert.Null(arrival.DueDay);
        Assert.Equal(new LocationState(null, 10), arrival.Next);
    }

    [Fact]
    public void The_timer_starts_on_the_visit_where_the_location_was_explored()
    {
        // Half opened on day 8, noticed when you come back on day 15: the timer counts from day 8, so it is already due.
        var arrival = Arrive(today: 15, containers: 10, opened: 5, start: null, lastVisit: 8);

        Assert.True(arrival.Started);
        Assert.Equal(12, arrival.DueDay);
        Assert.True(arrival.Restock);
        Assert.Equal(new LocationState(null, 15), arrival.Next);
    }

    [Fact]
    public void Back_before_the_timer_runs_out_keeps_waiting()
    {
        var arrival = Arrive(today: 10, containers: 10, opened: 5, start: null, lastVisit: 9);

        Assert.True(arrival.Started);
        Assert.False(arrival.Restock);
        Assert.Equal(13, arrival.DueDay);
        Assert.Equal(new LocationState(9, 10), arrival.Next);

        var later = Arrive(today: 12, containers: 10, opened: 7, start: 9, lastVisit: 10);

        Assert.False(later.Started);
        Assert.False(later.Restock);
        Assert.Equal(13, later.DueDay);
        Assert.Equal(new LocationState(9, 12), later.Next);
    }

    [Fact]
    public void Restocks_on_the_due_day_and_then_waits_to_be_explored_again()
    {
        var arrival = Arrive(today: 13, containers: 10, opened: 6, start: 9, lastVisit: 12);

        Assert.True(arrival.Restock);
        Assert.Equal(new LocationState(null, 13), arrival.Next);
    }

    [Fact]
    public void A_shorter_range_applies_to_a_timer_already_counting()
    {
        // The reported bug: the Tech Company's timer started at 6-9 days, then the player set it to 1 and came back
        // every day, and it never restocked because the timer kept the day it first rolled.
        var started = Arrive(today: 10, containers: 20, opened: 20, start: null, lastVisit: null, settings: SchoolDays(6, 9));

        Assert.InRange(started.DueDay!.Value, 16, 19);

        var nextDay = Arrive(today: 11, containers: 20, opened: 20, start: started.Next.StartDay, lastVisit: 10, settings: SchoolDays(1, 1));

        Assert.Equal(11, nextDay.DueDay);
        Assert.True(nextDay.Restock);
    }

    [Fact]
    public void A_longer_range_applies_to_a_timer_already_counting()
    {
        var arrival = Arrive(today: 13, containers: 10, opened: 6, start: 9, lastVisit: 12, settings: SchoolDays(20, 20));

        Assert.False(arrival.Restock);
        Assert.Equal(29, arrival.DueDay);
        Assert.Equal(new LocationState(9, 13), arrival.Next);
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

        Assert.Null(state.StartDay);
    }

    [Fact]
    public void Nothing_opened_means_nothing_to_restock_even_when_due()
    {
        var arrival = Arrive(today: 20, containers: 10, opened: 0, start: 9, lastVisit: 12);

        Assert.False(arrival.Restock);
        Assert.Equal(new LocationState(9, 20), arrival.Next);
    }

    [Fact]
    public void With_no_visit_on_record_the_timer_counts_from_today()
    {
        // The mod was added to a save where the school is already half opened.
        var arrival = Arrive(today: 30, containers: 10, opened: 8, start: null, lastVisit: null);

        Assert.True(arrival.Started);
        Assert.Equal(new LocationState(30, 30), arrival.Next);
        Assert.Equal(34, arrival.DueDay);
        Assert.False(arrival.Restock);
    }

    [Fact]
    public void A_location_without_its_own_range_uses_the_other_range()
    {
        var arrival = Arrive(today: 5, containers: 4, opened: 2, start: null, lastVisit: 5, map: 1099);

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

        Assert.Equal(5, Locations.Known.Select(l => l.MapId).Distinct().Count());
    }

    [Fact]
    public void No_setting_for_a_location_the_player_cannot_reach()
    {
        // Map 1001 is in the game's data but has no playable map; a setting for it would do nothing.
        Assert.DoesNotContain(Locations.Known, l => l.MapId == 1001);
    }
}

public class OldTimerTests
{
    private const int Tech = 1003;

    private static RestockSettings TechDays(int min, int max) =>
        new(true, 0.5, new Dictionary<int, DayRange> { [Tech] = new(min, max) }, Locations.OtherDefault);

    [Fact]
    public void An_unchanged_range_keeps_the_old_due_day()
    {
        var settings = TechDays(6, 9);

        for (var startDay = 0; startDay < 200; startDay++)
        {
            var oldDue = startDay + RestockRules.Roll(Tech, startDay, settings.RangeFor(Tech));
            var start = RestockRules.StartOfOldTimer(Tech, oldDue, startDay + 2, settings);

            Assert.Equal(oldDue, RestockRules.DueDay(Tech, start, settings));
        }
    }

    [Fact]
    public void A_range_set_to_one_day_after_the_timer_started_restocks_the_day_after_the_last_visit()
    {
        // Started on day 10 at 6-9 days, the player then set 1 and last visited on day 12.
        var oldDue = 10 + RestockRules.Roll(Tech, 10, new DayRange(6, 9));
        var start = RestockRules.StartOfOldTimer(Tech, oldDue, 12, TechDays(1, 1));

        Assert.Equal(13, RestockRules.DueDay(Tech, start, TechDays(1, 1)));
    }

    [Fact]
    public void A_changed_range_never_makes_an_old_timer_later_than_its_due_day()
    {
        foreach (var (min, max) in new[] { (1, 1), (2, 3), (3, 5), (10, 20), (30, 60) })
        {
            var settings = TechDays(min, max);

            // Late enough that day 0 is not in the way (see the test below).
            for (var startDay = 60; startDay < 110; startDay++)
            {
                var oldDue = startDay + RestockRules.Roll(Tech, startDay, new DayRange(6, 9));
                var start = RestockRules.StartOfOldTimer(Tech, oldDue, startDay + 1, settings);

                Assert.True(RestockRules.DueDay(Tech, start, settings) <= oldDue, $"{min}-{max} from day {startDay}");
            }
        }
    }

    [Fact]
    public void An_old_timer_near_the_first_day_never_starts_before_day_zero()
    {
        // A start before day 0 would be saved as no timer at all.
        Assert.Equal(0, RestockRules.StartOfOldTimer(Tech, 5, 1, TechDays(30, 60)));
    }

    [Fact]
    public void With_no_visit_on_record_the_old_due_day_is_the_limit()
    {
        var settings = TechDays(6, 9);
        var start = RestockRules.StartOfOldTimer(Tech, 40, null, settings);

        Assert.InRange(RestockRules.DueDay(Tech, start, settings), 37, 40);
    }
}
