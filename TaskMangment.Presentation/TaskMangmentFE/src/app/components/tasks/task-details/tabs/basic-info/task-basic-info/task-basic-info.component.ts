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
  @Input() readonly = false;
hasExtensions = false;
hasNewDate = false;
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
      next: (res) => {
        this.taskInfo = res.data;
         this.hasExtensions = !!this.taskInfo?.numberOfExtensions && this.taskInfo.numberOfExtensions > 0;
         this.hasNewDate = !!this.taskInfo?.newDate;
        if (this.taskInfo) {
          this.form.patchValue({
            title: this.taskInfo.title,
            description: this.taskInfo.description,
            commentAllowPeriodDays: this.taskInfo.commentAllowPeriodDays,
            maxWarningsBeforeDiscount: this.taskInfo.maxWarningsBeforeDiscount,
            penaltyOnAutoClose: this.taskInfo.penaltyOnAutoClose,
            penaltyOnStopComment: this.taskInfo.penaltyOnStopComment,
            createdDate:this.taskInfo.createdDate,
            dueDate: this.taskInfo.dueDate,
            assignedByName: this.taskInfo.assignedByName,
            newDate: this.taskInfo.newDate,
            numberOfExtensions: this.taskInfo.numberOfExtensions
           // closedAt: this.taskInfo.ClosedAt
          });
        }
      },
      error: (err) => {
        console.error('Failed to load task:', err);
      }
    });
  }

  
}
