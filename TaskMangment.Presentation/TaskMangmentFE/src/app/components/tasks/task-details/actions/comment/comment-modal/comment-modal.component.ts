import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, NgModel, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskCommentService } from 'app/core/services/task-comment.service';
import { ToastrService } from 'ngx-toastr';
import { CalendarEventService } from 'app/core/services/calendar-events.service';
import { CalendarEventType } from 'app/core/models/event/calendar';

@Component({
  selector: 'app-comment-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule,FormsModule],
  templateUrl: './comment-modal.component.html'
})
export class CommentModalComponent implements OnInit {

  @Input() taskId!: number;

  form!: FormGroup;
  files: File[] = [];
  isSubmitting = false;
showEventFields = false;
  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private commentService: TaskCommentService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private calendarService: CalendarEventService
    
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      comment: [null, [Validators.minLength(5)]],
       startDate: [null],  
       endDate: [null]
    });
  }

  onFileChange(event: any) {
  if (event.target.files && event.target.files.length > 0) {
    this.files = Array.from(event.target.files);
  } else {
    this.files = [];
  }

  const commentControl = this.form.get('comment');

  if (!commentControl) return;

  if (this.files.length > 0) {
    commentControl.clearValidators();
  } else {
    commentControl.setValidators([
      Validators.required,
      Validators.minLength(5)
    ]);
  }

  commentControl.updateValueAndValidity();
}

submit(): void {
  if (this.form.invalid || this.isSubmitting) return;

  const formData = new FormData();

  // Comment text
  const commentText = this.form.value.comment?.trim() || '';
  formData.append('CommentText', commentText);

  if (!commentText && this.files.length === 0) {
  this.toastr.error(this.translate.instant('TASK.COMMENT_OR_FILE_REQUIRED'));
    this.isSubmitting = true;
  return;
}
  // Attachments
  this.files.forEach((file) => {
    formData.append('File', file);
  });

  this.isSubmitting = true;

  // إنشاء التعليق
  this.commentService.create(this.taskId, formData).subscribe({
    next: () => {
      if (this.form.value.startDate) {
        const eventPayload = {
          title: 'Comment',
          description: commentText,
          startDate: new Date(this.form.value.startDate).toISOString(),
          endDate: this.form.value.endDate ? new Date(this.form.value.endDate).toISOString() : null,
          allDay: false,
          eventType: CalendarEventType.Comment,
          reminder: 15
        };

        this.calendarService.create(eventPayload).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('TASK.COMMENT_SUCCESS'));
            this.isSubmitting = false;
            this.modal.close(true);
          },
          error: () => {
            this.isSubmitting = false;
            this.toastr.error(this.translate.instant('TASK.COMMENT_EVENT_ERROR'));
          }
        });
      } else {
        // لو مفيش startDate → نغلق المودال بعد التعليق فقط
        this.toastr.success(this.translate.instant('TASK.COMMENT_SUCCESS'));
        this.isSubmitting = false;
        this.modal.close(true);
      }
    },
    error: () => {
      this.isSubmitting = false;
      this.toastr.error(this.translate.instant('TASK.COMMENT_ERROR'));
    }
  });
}
  toggleEventFields() {
    this.showEventFields = !this.showEventFields;
  }


}
