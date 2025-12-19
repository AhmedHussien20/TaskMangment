import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { TaskCommentService } from 'app/core/services/task-comment.service';

@Component({
  selector: 'app-comment-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './comment-modal.component.html'
})
export class CommentModalComponent implements OnInit {

  @Input() taskId!: number;

  form!: FormGroup;
  files: File[] = [];
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private commentService: TaskCommentService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      comment: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  onFileChange(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.files = Array.from(event.target.files);
    }
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const formData = new FormData();
    formData.append('CommentText', this.form.value.comment.trim());

    this.files.forEach((file) => {
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
