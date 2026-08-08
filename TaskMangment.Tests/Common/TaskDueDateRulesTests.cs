using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Validation;

namespace TaskMangment.Tests.Common;

public class TaskDueDateRulesTests
{
    [Theory]
    [InlineData(2026, 7, 24)] // Friday
    [InlineData(2026, 7, 25)] // Saturday
    public void IsWeekend_FridayAndSaturday_ReturnsTrue(int y, int m, int d)
    {
        Assert.True(TaskDueDateRules.IsWeekend(new DateTime(y, m, d)));
    }

    [Theory]
    [InlineData(2026, 7, 26)] // Sunday
    [InlineData(2026, 7, 27)] // Monday
    [InlineData(2026, 7, 28)] // Tuesday
    [InlineData(2026, 7, 29)] // Wednesday
    [InlineData(2026, 7, 30)] // Thursday
    public void IsWeekend_Workdays_ReturnsFalse(int y, int m, int d)
    {
        Assert.False(TaskDueDateRules.IsWeekend(new DateTime(y, m, d)));
    }

    [Fact]
    public void EnsureNotWeekend_Null_DoesNotThrow()
    {
        TaskDueDateRules.EnsureNotWeekend(null);
    }

    [Fact]
    public void EnsureNotWeekend_Thursday_DoesNotThrow()
    {
        TaskDueDateRules.EnsureNotWeekend(new DateTime(2026, 7, 30));
    }

    [Fact]
    public void EnsureNotWeekend_Friday_ThrowsDueDateOnWeekend()
    {
        var ex = Assert.Throws<AppException>(() =>
            TaskDueDateRules.EnsureNotWeekend(new DateTime(2026, 7, 24)));

        Assert.Equal(ErrorCodes.DueDateOnWeekend, ex.ErrorCode);
    }

    [Fact]
    public void EnsureNotPast_Yesterday_ThrowsDueDateInPast()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var ex = Assert.Throws<AppException>(() =>
            TaskDueDateRules.EnsureNotPast(yesterday));

        Assert.Equal(ErrorCodes.DueDateInPast, ex.ErrorCode);
    }

    [Fact]
    public void EnsureValidDueDate_TodayWeekday_DoesNotThrow()
    {
        var date = DateTime.UtcNow.Date;
        while (TaskDueDateRules.IsWeekend(date))
            date = date.AddDays(1);

        TaskDueDateRules.EnsureValidDueDate(date);
    }
}
