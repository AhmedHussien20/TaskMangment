import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TaskService } from 'app/core/services/task.service';
import { TaskGet } from 'app/core/models/task/task';
import { TranslateModule } from '@ngx-translate/core';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';
@Component({
  selector: 'app-task-basic-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,TranslateModule, MyDatePipe
],
  templateUrl: './task-basic-info.component.html',
    styleUrls: ['./task-basic-info.component.scss']


})
export class TaskBasicInfoComponent implements OnChanges {

  @Input() taskId!: number;
  /** When provided by the shell, reuse it and skip an extra getById. */
  @Input() taskInfo: TaskGet | null = null;
  @Input() readonly = false;
hasExtensions = false;
hasNewDate = false;
  form!: FormGroup;

  constructor(private fb: FormBuilder, private taskService: TaskService) {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['taskInfo'] && this.taskInfo) {
      this.applyTask(this.taskInfo);
      return;
    }
    // Fallback only when used without a parent-provided taskInfo.
    if (changes['taskId'] && this.taskId && this.taskInfo == null && !('taskInfo' in (changes))) {
      this.loadTask();
    }
  }

  buildForm() {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      commentAllowPeriodDays: [''],
      maxWarningsBeforeDiscount: [3],
      penaltyOnAutoClose: [0],
      penaltyOnStopComment: [0],
      createdDate:[null],
      dueDate: [null],
      assignedByName: [''],
      newDate: [null],
      numberOfExtensions: [null]
    });
  }

  loadTask() {
    if (!this.taskId) return;

    this.taskService.getById(this.taskId).subscribe({
      next: (res) => this.applyTask(res.data),
      error: (err) => {
        console.error('Failed to load task:', err);
      }
    });
  }

  private applyTask(task: TaskGet | null): void {
    if (!task) return;
    this.taskInfo = task;
    this.hasExtensions = !!task.numberOfExtensions && task.numberOfExtensions > 0;
    this.hasNewDate = !!task.newDate;
    this.form.patchValue({
      title: task.title,
      description: task.description,
      commentAllowPeriodDays: task.commentAllowPeriodDays,
      maxWarningsBeforeDiscount: task.maxWarningsBeforeDiscount,
      penaltyOnAutoClose: task.penaltyOnAutoClose,
      penaltyOnStopComment: task.penaltyOnStopComment,
      createdDate: task.createdDate,
      dueDate: task.dueDate,
      assignedByName: task.assignedByName,
      newDate: task.newDate,
      numberOfExtensions: task.numberOfExtensions
    });
  }
}
