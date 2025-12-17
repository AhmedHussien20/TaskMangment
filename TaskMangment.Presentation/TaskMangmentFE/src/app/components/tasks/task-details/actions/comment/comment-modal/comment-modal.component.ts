import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-comment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <div class="modal-header">
      <h6 class="modal-title">{{ 'TASK.ADD_COMMENT' | translate }}</h6>
      <button class="btn-close" (click)="modal.dismiss()"></button>
    </div>

    <div class="modal-body">
      <textarea
        class="form-control"
        rows="4"
        [(ngModel)]="comment"
        placeholder="{{ 'TASK.COMMENT_PLACEHOLDER' | translate }}">
      </textarea>
    </div>

    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="modal.dismiss()">
        {{ 'FORM.CANCEL' | translate }}
      </button>

      <button class="btn btn-primary" (click)="submit()">
        {{ 'FORM.SAVE' | translate }}
      </button>
    </div>
  `
})
export class CommentModalComponent {

  @Input() taskId!: number;

  comment = '';

  constructor(public modal: NgbActiveModal) {}

  submit() {
    if (!this.comment.trim()) return;

    // 🔹 هنا بعدين هنربط API
    console.log('Save comment:', this.comment, 'for task', this.taskId);

    this.modal.close(true); // success
  }
}
