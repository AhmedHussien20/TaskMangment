using TaskMangment.Application.Common.Validation;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Tests.Security;

public class TaskReopenRulesTests
{
    [Theory]
    [InlineData(WorkTaskStatus.Closed, true)]
    [InlineData(WorkTaskStatus.AutoClose, true)]
    [InlineData(WorkTaskStatus.Archived, false)]
    [InlineData(WorkTaskStatus.InProgress, false)]
    [InlineData(WorkTaskStatus.New, false)]
    public void IsReopenable_OnlyClosedAndAutoClose(WorkTaskStatus status, bool expected)
    {
        Assert.Equal(expected, TaskReopenRules.IsReopenable(status));
    }

    [Fact]
    public void Reopen_KeepsHistoryAndDoesNotTouchDiscount()
    {
        var closedAt = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 11, 20);
        var task = new WorkTask
        {
            Id = 7,
            Status = WorkTaskStatus.Closed,
            ClosedAt = closedAt,
            ClosedByUserId = 4,
            CloseReason = CloseReason.Manual,
            DueDate = dueDate
        };
        var assignment = new TaskAssignment { TaskId = 7, EmployeeId = 9, IsClosed = true, IsActive = true };
        var discount = new Discount
        {
            Id = 3,
            TaskId = 7,
            EmployeeId = 9,
            Amount = 150m,
            Reason = "AUTO_CLOSE_DISCOUNT",
            discountType = DiscountType.AutoCloseTaskDiscount,
            IsDeleted = false
        };

        var history = TaskReopenRules.Capture(task, reopenedByEmployeeId: 10, newDueDate: null, reopenedAt: DateTime.UtcNow);
        TaskReopenRules.ApplyOpenState(task, new[] { assignment }, newDueDate: null);

        Assert.Equal(WorkTaskStatus.Closed, history.PreviousStatus);
        Assert.Equal(closedAt, history.PreviousClosedAt);
        Assert.Equal(CloseReason.Manual, history.PreviousCloseReason);
        Assert.Equal(dueDate, history.PreviousDueDate);
        Assert.Null(history.NewDueDate);
        Assert.Equal(10, history.ReopenedByEmployeeId);

        Assert.Equal(WorkTaskStatus.InProgress, task.Status);
        Assert.Null(task.ClosedAt);
        Assert.Null(task.ClosedByUserId);
        Assert.Null(task.CloseReason);
        Assert.False(assignment.IsClosed);

        Assert.Equal(150m, discount.Amount);
        Assert.Equal(DiscountType.AutoCloseTaskDiscount, discount.discountType);
        Assert.False(discount.IsDeleted);
        Assert.Equal("AUTO_CLOSE_DISCOUNT", discount.Reason);
    }
}
