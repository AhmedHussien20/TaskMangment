import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TaskService } from 'app/core/services/task.service';
import { TaskGet } from 'app/core/models/task/task';
import { TranslateModule } from '@ngx-translate/core';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-task-basic-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,TranslateModule, MyDatePipe, DatePipe
],
  templateUrl: './task-basic-info.component.html'
})
export class TaskBasicInfoComponent implements OnChanges {

  @Input() taskId!: number;
  @Input() readonly = false;

  form!: FormGroup;
  taskInfo!: TaskGet | null;

  constructor(private fb: FormBuilder, private taskService: TaskService) {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['taskId'] && this.taskId) {
      this.loadTask();
    }
  }

  buildForm() {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      commentAllowPeriodDays: [''],
      maxWarnings: [0],
      penaltyAtMaxWarnings: [0],
      penaltyOnAutoClose: [0],
      dueDate: [null],
      assignedByName: ['']
    });
  }

  loadTask() {
    if (!this.taskId) return;

    this.taskService.getById(this.taskId).subscribe({
      next: (res) => {
        this.taskInfo = res.data;
        if (this.taskInfo) {
          this.form.patchValue({
            title: this.taskInfo.title,
            description: this.taskInfo.description,
            commentAllowPeriodDays: this.taskInfo.commentAllowPeriodDays,
            maxWarnings: this.taskInfo.maxWarnings,
            penaltyAtMaxWarnings: this.taskInfo.penaltyAtMaxWarnings,
            penaltyOnAutoClose: this.taskInfo.penaltyOnAutoClose,
            dueDate: this.taskInfo.dueDate,
            assignedByName: this.taskInfo.assignedByName
          });
        }
      },
      error: (err) => {
        console.error('Failed to load task:', err);
      }
    });
  }

  
}
