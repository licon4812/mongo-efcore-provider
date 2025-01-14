﻿/* Copyright 2023-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MongoDB.EntityFrameworkCore.FunctionalTests.Entities.Guides;

namespace MongoDB.EntityFrameworkCore.FunctionalTests.Query;

public static class SkipTakeOrderingTests
{
    private static readonly GuidesDbContext __db = GuidesDbContext.Create(TestServer.GetClient());

    [Fact]
    public static void Take_with_constant_integer()
    {
        var results = __db.Planets.Take(3).ToArray();

        Assert.Equal(3, results.Length);
        Assert.All(results, r =>
        {
            Assert.NotNull(r.name);
            Assert.InRange(r.orderFromSun, 1, 8);
        });
    }

    [Fact]
    public static void Skip_with_constant_integer()
    {
        var results = __db.Planets.Skip(3).ToArray();

        Assert.Equal(5, results.Length);
        Assert.All(results, r =>
        {
            Assert.NotNull(r.name);
            Assert.InRange(r.orderFromSun, 1, 8);
        });
    }

    [Fact]
    public static void Skip_with_zero_integer()
    {
        var results = __db.Planets.Skip(0).ToArray();

        Assert.Equal(8, results.Length);
        Assert.All(results, r =>
        {
            Assert.NotNull(r.name);
            Assert.InRange(r.orderFromSun, 1, 8);
        });
    }

    [Fact]
    public static void SkipTake_with_constant_integer()
    {
        // If this test is flaky due to non-deterministic ordering
        // add an OrderBy (but would prefer it without it for isolation).
        var skipped = __db.Planets.Take(3).ToArray();
        var results = __db.Planets.Skip(3).Take(3).ToArray();

        Assert.Equal(3, results.Length);
        Assert.All(results, r => Assert.DoesNotContain(r, skipped));
    }

    [Fact]
    public static void OrderBy_string()
    {
        var results = __db.Planets.OrderBy(o => o.name).ToArray();

        string previousName = "";

        Assert.Equal(8, results.Length);
        Assert.All(results, r =>
        {
            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) > 0);
            previousName = r.name;
        });
    }

    [Fact]
    public static void OrderByDescending_string()
    {
        var results = __db.Planets.OrderByDescending(o => o.name).ToArray();

        string previousName = "zzz";

        Assert.Equal(8, results.Length);
        Assert.All(results, r =>
        {
            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) < 0);
            previousName = r.name;
        });
    }

    [Fact]
    public static void OrderBy_integer()
    {
        var results = __db.Planets.OrderBy(o => o.orderFromSun).ToArray();

        int previousOrderFromSun = 0;

        Assert.Equal(8, results.Length);
        Assert.All(results, r =>
        {
            Assert.True(r.orderFromSun > previousOrderFromSun);
            previousOrderFromSun = r.orderFromSun;
        });
    }

    [Fact]
    public static void OrderByDescending_integer()
    {
        var results = __db.Planets.OrderByDescending(o => o.orderFromSun).ToArray();

        int previousOrderFromSun = 9999;

        Assert.Equal(8, results.Length);
        Assert.All(results, r =>
        {
            Assert.True(r.orderFromSun < previousOrderFromSun);
            previousOrderFromSun = r.orderFromSun;
        });
    }

    [Fact]
    public static void OrderBy_boolean()
    {
        var results = __db.Planets.OrderBy(o => o.hasRings).ToArray();

        Assert.Equal(8, results.Length);
        Assert.False(results.First().hasRings);
        Assert.True(results.Last().hasRings);
        bool changed = false;
        bool previousHasRings = false;
        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(changed);
                previousHasRings = r.hasRings;
                changed = true;
            }
        });
    }

    [Fact]
    public static void OrderByDescending_boolean()
    {
        var results = __db.Planets.OrderByDescending(o => o.hasRings).ToArray();

        Assert.Equal(8, results.Length);
        Assert.True(results.First().hasRings);
        Assert.False(results.Last().hasRings);
        bool changed = false;
        bool previousHasRings = true;
        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(changed);
                previousHasRings = r.hasRings;
                changed = true;
            }
        });
    }

    [Fact]
    public static void OrderByThenBy()
    {
        var results = __db.Planets.OrderBy(o => o.hasRings).ThenBy(o => o.name).ToArray();

        Assert.Equal(8, results.Length);
        Assert.False(results.First().hasRings);
        Assert.True(results.Last().hasRings);
        bool ringsChanged = false;
        bool previousHasRings = false;
        string previousName = "";

        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(ringsChanged);
                previousHasRings = r.hasRings;
                ringsChanged = true;
                previousName = "";
            }

            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) > 0);
            previousName = r.name;
        });
    }

    [Fact]
    public static void OrderByThenByDescending()
    {
        var results = __db.Planets.OrderBy(o => o.hasRings).ThenByDescending(o => o.name).ToArray();

        Assert.Equal(8, results.Length);
        Assert.False(results.First().hasRings);
        Assert.True(results.Last().hasRings);
        bool ringsChanged = false;
        bool previousHasRings = false;
        string previousName = "zzzz";
        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(ringsChanged);
                previousHasRings = r.hasRings;
                ringsChanged = true;
                previousName = "zzzz";
            }

            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) < 0);
            previousName = r.name;
        });
    }

    [Fact]
    public static void OrderByDescendingThenBy()
    {
        var results = __db.Planets.OrderByDescending(o => o.hasRings).ThenBy(o => o.name).ToArray();

        Assert.Equal(8, results.Length);
        Assert.True(results.First().hasRings);
        Assert.False(results.Last().hasRings);
        bool ringsChanged = false;
        bool previousHasRings = true;
        string previousName = "";
        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(ringsChanged);
                previousHasRings = r.hasRings;
                ringsChanged = true;
                previousName = "";
            }

            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) > 0);
            previousName = r.name;
        });
    }

    [Fact]
    public static void OrderByDescendingThenByDescending()
    {
        var results = __db.Planets.OrderByDescending(o => o.hasRings).ThenByDescending(o => o.name).ToArray();

        Assert.Equal(8, results.Length);
        Assert.True(results.First().hasRings);
        Assert.False(results.Last().hasRings);
        bool ringsChanged = false;
        bool previousHasRings = true;
        string previousName = "zzzz";
        Assert.All(results, r =>
        {
            if (r.hasRings != previousHasRings)
            {
                Assert.False(ringsChanged);
                previousHasRings = r.hasRings;
                ringsChanged = true;
                previousName = "zzzz";
            }

            Assert.True(string.Compare(r.name, previousName, StringComparison.Ordinal) < 0);
            previousName = r.name;
        });
    }
}
