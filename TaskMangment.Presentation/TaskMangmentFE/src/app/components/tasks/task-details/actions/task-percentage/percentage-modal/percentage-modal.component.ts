import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskPercentageService } from 'app/core/services/task-percentage.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-percentage-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './percentage-modal.component.html'
})
export class PercentageModalComponent implements OnInit {

  @Input() taskId!: number;
  @Input() employeeId?: number;

  form!: FormGroup;
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private percentageService: TaskPercentageService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      achievementPercent: ['', [Validators.required, Validators.pattern('^(100(\\.0+)?|\\d{1,2}(\\.\\d+)?)%?$')]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const model = { achievementPercent: this.form.value.achievementPercent.trim() };
    this.isSubmitting = true;

    this.percentageService.create(this.taskId, model).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.PERCENTAGE_SUCCESS'));
        this.isSubmitting = false;
        this.modal.close(true);
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }
}
