import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-extend-request',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule
  ],
  templateUrl: './extend-request.component.html',
  styleUrls: ['./extend-request.component.scss']
})
export class ExtendRequestComponent {

  @Input() taskId!: number;
  @Output() submitted = new EventEmitter<void>();

  reason: string = '';

  submit(): void {
    if (!this.reason?.trim()) return;

    // TODO: API call
    this.submitted.emit();
  }
}
