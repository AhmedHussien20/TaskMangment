import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TaskService } from 'app/core/services/task.service';
import { TaskGet, TaskPriority, TaskStatus } from 'app/core/models/task/task';
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

  priorityLabelKey = 'TASK.PRIORITY_LOW';
  priorityBadgeClass = 'bg-success';
  statusLabelKey = 'TASK.STATUS_NEW';
  statusBadgeClass = 'bg-secondary';

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
      minCommentsPerPeriod: [1],
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
    this.setPriorityDisplay(task);
    this.setStatusDisplay(task);
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

  private setPriorityDisplay(task: TaskGet): void {
    const key = (task as any).priorityText ?? TaskPriority[task.priority as TaskPriority] ?? task.priority;
    switch (String(key)) {
      case '3':
      case 'High':
        this.priorityLabelKey = 'TASK.PRIORITY_HIGH';
        this.priorityBadgeClass = 'bg-danger';
        break;
      case '2':
      case 'Medium':
        this.priorityLabelKey = 'TASK.PRIORITY_MEDIUM';
        this.priorityBadgeClass = 'bg-warning';
        break;
      default:
        this.priorityLabelKey = 'TASK.PRIORITY_LOW';
        this.priorityBadgeClass = 'bg-success';
        break;
    }
  }

  private setStatusDisplay(task: TaskGet): void {
    const key = (task as any).statusText ?? TaskStatus[task.status as TaskStatus] ?? task.status;
    switch (String(key)) {
      case '2':
      case 'InProgress':
        this.statusLabelKey = 'TASK.STATUS_IN_PROGRESS';
        this.statusBadgeClass = 'bg-info';
        break;
      case '3':
      case 'Closed':
        this.statusLabelKey = 'TASK.STATUS_CLOSED';
        this.statusBadgeClass = 'bg-success';
        break;
      case '4':
      case 'Archived':
        this.statusLabelKey = 'TASK.STATUS_ARCHIVED';
        this.statusBadgeClass = 'bg-dark';
        break;
      case '5':
      case 'AutoClose':
        this.statusLabelKey = 'TASK.STATUS_AUTOCLOSE';
        this.statusBadgeClass = 'bg-warning';
        break;
      default:
        this.statusLabelKey = 'TASK.STATUS_NEW';
        this.statusBadgeClass = 'bg-secondary';
        break;
    }
  }
}
