using Microsoft.Extensions.Localization;

using Microsoft.EntityFrameworkCore;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using TaskMangment.Application.Common.Interfaces;

using TaskMangment.Application.Common.Notification;

using TaskMangment.Application.Interfaces.IRepository;

using TaskMangment.Application.Interfaces.Services;

using TaskMangment.Domain.Entities;

using TaskMangment.Domain.Event;

using TaskMangment.Utilities.Localization.Resources;



namespace TaskMangment.Application.Behaviors

{

    public class TaskCommentEventHandler : IEventHandler<TaskCommentAddedEvent>

    {

        private readonly INotificationService _notificationService;

        private readonly IStringLocalizer<TaskNotification> _localizer;

        private readonly IRepository<TaskComment> _commentRepo;

        private readonly IRepository<Attachment> _attachmentRepo;

        private readonly IBlobStorageService _blobStorageService;



        public TaskCommentEventHandler(

            INotificationService notificationService,

             IStringLocalizer<TaskNotification> localizer,

             IRepository<TaskComment> commentRepo,

             IRepository<Attachment> attachmentRepo,

             IBlobStorageService blobStorageService)

        {

            _notificationService = notificationService;

            _localizer = localizer;

            _commentRepo = commentRepo;

            _attachmentRepo = attachmentRepo;

            _blobStorageService = blobStorageService;

        }



        public async Task Handle(TaskCommentAddedEvent ev)

        {

            var messageTemplate = _localizer[

                NotificationCode.TaskCommentNotification

            ];



            var message = string.Format(

                messageTemplate,

                ev.TaskId,

                ev.taskTitle,

                ev.EmployeeName

            );

            var commentText = await _commentRepo

                .GetAll(c => c.Id == ev.CommentId)

                .Select(c => c.CommentText)

                .FirstOrDefaultAsync();

            var commentLabel = _localizer["COMMENT_LABEL"];

            var whatsAppMessage = string.IsNullOrWhiteSpace(commentText)

                ? message

                : $"{message}\n\n{commentLabel}:\n{commentText}";



            var attachments = await _attachmentRepo
                .GetAll(a =>
                    a.ReferenceId == ev.CommentId &&
                    a.AttachmentType == AttachmentType.Comment &&
                    !a.IsDeleted)
                .Select(a => new { a.FileName, a.FilePath, a.BlobUrl, a.ContentType })
                .ToListAsync();

            var whatsAppAttachments = attachments
                .Select(a =>
                {
                    var path = !string.IsNullOrWhiteSpace(a.BlobUrl) ? a.BlobUrl : a.FilePath;
                    return new WhatsAppAttachment
                    {
                        FileName = a.FileName,
                        Url = _blobStorageService.WithSas(path),
                        ContentType = a.ContentType
                    };
                })
                .Where(a => !string.IsNullOrWhiteSpace(a.Url))
                .ToList();



            foreach (var empId in ev.Recipients)

            {

                await _notificationService.SendAsync(

                    empId,

                    message,

                    sendEmail: true,

                    sendWhatsApp: true,

                    ev.TaskId,

                    NotificationType.Comments,

                    ev.CommentId,

                    whatsAppMessage,

                    whatsAppAttachments

                );

            }

        }

    }

}

