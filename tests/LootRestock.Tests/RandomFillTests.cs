using LootRestock.Core;
using Xunit;

namespace LootRestock.Tests;

public class RandomFillTests
{
    [Fact]
    public void A_random_fill_that_added_nothing_does_not_count_as_refilled()
    {
        // The container would be marked unopened and look full while it is empty.
        Assert.False(FixedRefill.GotRandomLoot(itemsBefore: 2, itemsAfter: 2));
        Assert.False(FixedRefill.GotRandomLoot(itemsBefore: 0, itemsAfter: 0));
    }

    [Fact]
    public void A_random_fill_that_added_items_counts_as_refilled()
    {
        Assert.True(FixedRefill.GotRandomLoot(itemsBefore: 1, itemsAfter: 3));
    }
}
