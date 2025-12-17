import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { TaskCommentAddEditDto } from 'app/core/models/task/task-comment';
import { TaskCommentService } from 'app/core/services/task-comment.service';

@Component({
  selector: 'app-comment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './comment-modal.component.html'
})
export class CommentModalComponent {

  @Input() taskId!: number;

  comment = '';
  selectedFile!: File | null;
  files: File[] = [];
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private commentService: TaskCommentService
  ) {}

  onFileChange(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.files = Array.from(event.target.files);
    }
  }

 submit() {
  if (!this.comment.trim() || this.isSubmitting) return;

  const formData = new FormData();
  formData.append('CommentText', this.comment.trim());

  // رفع جميع الملفات الموجودة في this.files
  this.files.forEach((file, index) => {
    // اسم الحقل File مهم ويجب مطابق DTO
    // لو حبيت تدعم رفع أكثر من ملف، استخدم File[index] مثلاً
    formData.append('File', file);
  });

  this.isSubmitting = true;

  this.commentService.create(this.taskId, formData).subscribe({
    next: () => {
      this.isSubmitting = false;
      this.modal.close(true);
    },
    error: (err) => {
      console.error('Failed to save comment', err);
      this.isSubmitting = false;
    }
  });
}

}
