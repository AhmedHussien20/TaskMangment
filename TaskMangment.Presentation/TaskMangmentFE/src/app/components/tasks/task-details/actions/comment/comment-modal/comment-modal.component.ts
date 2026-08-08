import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, NgModel, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskCommentService } from 'app/core/services/task-comment.service';
import { ToastrService } from 'ngx-toastr';
import { CalendarEventService } from 'app/core/services/calendar-events.service';
import { CalendarEventType } from 'app/core/models/event/calendar';
import { AuthService } from 'app/core/services/auth.service';

@Component({
  selector: 'app-comment-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule,FormsModule],
  templateUrl: './comment-modal.component.html'
})
export class CommentModalComponent implements OnInit {

  @Input() taskId!: number;
  @Input() requireUploadFile: boolean = false;
  @Input() createdByMe: boolean = false;

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
    private calendarService: CalendarEventService,
    private auth: AuthService
    
  ) {}

  ngOnInit(): void {
    console.log('from comment model',this.requireUploadFile);

  this.form = this.fb.group({
    comment: [''],
    startDate: [null],
    endDate: [null]
  },
   { validators: [this.fileRequiredValidator()] }
);

  this.form.get('comment')?.valueChanges.subscribe(() => {
    this.updateCommentValidators();
  });

  this.updateCommentValidators();
}
private fileRequiredValidator() {
  return (group: FormGroup) => {
    if (!this.requireUploadFile) return null;

    return this.files.length > 0 ? null : { fileRequired: true };
  };
}

onFileChange(event: any) {
  const input = event.target as HTMLInputElement;

  this.files = input.files ? Array.from(input.files) : [];

  this.updateCommentValidators();
  this.form.updateValueAndValidity({ emitEvent: false });
}


private updateCommentValidators() {
const regex = /^(?!.*\..*\.)[a-zA-Z0-9\u0600-\u06FF .]+$/;
  const commentControl = this.form.get('comment');
  if (!commentControl) return;

  if (this.requireUploadFile) {
    commentControl.clearValidators();
  }
  else {
    if (this.files.length > 0 || this.createdByMe) {
      commentControl.clearValidators();
    } else {
      commentControl.setValidators([
        Validators.required,
        Validators.minLength(200),
        Validators.pattern(regex)

      ]);
    }
  }

  commentControl.updateValueAndValidity({ emitEvent: false });
}


submit(): void {
  if (this.form.invalid || this.isSubmitting) return;

  const commentText = this.form.value.comment?.trim() || '';
  const hasFiles = this.files.length > 0;

  if (this.requireUploadFile && !hasFiles) {
    this.toastr.error(this.translate.instant('TASK.FILE_REQUIRED'));
    return;
  }

  if (!this.requireUploadFile && !commentText && !hasFiles) {
    this.toastr.error(this.translate.instant('TASK.COMMENT_OR_FILE_REQUIRED'));
    return;
  }

  const formData = new FormData();
  formData.append('CommentText', commentText);

this.files.forEach(file => formData.append('Files', file));

  this.isSubmitting = true;

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
        this.toastr.success(this.translate.instant('TASK.COMMENT_SUCCESS'));
        this.isSubmitting = false;
        this.modal.close(true);
      }
    },
    error: () => {
      this.isSubmitting = false;
    }
  });
}
  toggleEventFields() {
    this.showEventFields = !this.showEventFields;
  }


}
