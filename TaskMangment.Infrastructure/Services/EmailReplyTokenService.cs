using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

public class EmailReplyTokenService : IEmailReplyTokenService
{
    private readonly AppDbContext _db;

    public EmailReplyTokenService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> CreateTaskReplyTokenAsync(int taskId, int commentId)
    {
        var token = Guid.NewGuid().ToString("N");

        _db.EmailReplyMaps.Add(new EmailReplyMap
        {
            Token = token,
            TaskId = taskId,
            OriginalCommentId = commentId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return token;
    }
}
