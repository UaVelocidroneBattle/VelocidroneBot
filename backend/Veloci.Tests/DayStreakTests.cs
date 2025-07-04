using FluentAssertions;
using Veloci.Data.Domain;

namespace Veloci.Tests;

public class DayStreakTests
{
    [Fact]
    public void increase_day_streak_should_increase_streak()
    {
        // Arrange
        var pilot = new Pilot("TestPilot");
        var today = DateTime.Now;

        // Act
        pilot.OnRaceFlown(today);

        // Assert
        pilot.DayStreak.Should().Be(1);
        pilot.LastRaceDate.Should().Be(today);
    }

    [Fact]
    public void increase_streak_from_1_freezes_should_be_0()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 1,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        var today = DateTime.Now;
        pilot.OnRaceFlown(today);

        // Assert
        pilot.DayStreak.Should().Be(2);
        pilot.DayStreakFreezeCount.Should().Be(0);
    }

    [Fact]
    public void increase_streak_from_29_freezes_should_be_1()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 29,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        var today = DateTime.Now;
        pilot.OnRaceFlown(today);

        // Assert
        pilot.DayStreak.Should().Be(30);
        pilot.DayStreakFreezeCount.Should().Be(1);
    }

    [Fact]
    public void increase_streak_from_59_freezes_should_be_1()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 59,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        var today = DateTime.Now;
        pilot.OnRaceFlown(today);

        // Assert
        pilot.DayStreak.Should().Be(60);
        pilot.DayStreakFreezeCount.Should().Be(1);
    }

    [Fact]
    public void reset_streak_when_has_2_freezes()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 50,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        pilot.DayStreakFreezes.Add(new DayStreakFreeze());
        pilot.DayStreakFreezes.Add(new DayStreakFreeze());

        pilot.ResetDayStreak(DateTime.Now);

        // Assert
        pilot.DayStreak.Should().Be(50);
        pilot.DayStreakFreezeCount.Should().Be(1);
    }

    /// <summary>
    /// It should be ok to call ResetDayStreak multiple times during one day.
    /// </summary>
    [Fact]
    public void reset_streak_can_be_called_multiple_times()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 50,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        pilot.DayStreakFreezes.Add(new DayStreakFreeze());
        pilot.DayStreakFreezes.Add(new DayStreakFreeze());

        pilot.ResetDayStreak(DateTime.Now);
        pilot.ResetDayStreak(DateTime.Now);

        // Assert
        pilot.DayStreak.Should().Be(50);
        pilot.DayStreakFreezeCount.Should().Be(1);
    }

    [Fact]
    public void reset_streak_when_has_0_freezes()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 50,
            DayStreakFreezes = new List<DayStreakFreeze>()
        };

        pilot.ResetDayStreak(DateTime.Now);

        // Assert
        pilot.DayStreak.Should().Be(0);
        pilot.DayStreakFreezeCount.Should().Be(0);
    }
}
