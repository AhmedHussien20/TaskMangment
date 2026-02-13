import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TaskTabsComponent } from '../task-tabs/task-tabs.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { CommentModalComponent } from '../actions/comment/comment-modal/comment-modal.component';
import { NgbActiveModal, NgbModal, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { TaskDetailsRefreshService } from '../task-details-refresh.service';
import { WarningModalComponent } from '../actions/warning/warning-modal/warning-modal.component';
import { PenaltyModalComponent } from '../actions/penalty/penalty-modal/penalty-modal.component';
import { CloseRequestModalComponent } from '../actions/close-request/close-request-modal/close-request-modal.component';
import { ExtendRequestComponent } from '../actions/extend-request/extend-request.component';
import { TaskService } from 'app/core/services/task.service';
import { TaskGet } from 'app/core/models/task/task';
import { AuthService } from 'app/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { PercentageModalComponent } from '../actions/task-percentage/percentage-modal/percentage-modal.component';

@Component({
  selector: 'app-task-details-shell',
  standalone: true,
  templateUrl: './task-details-shell.component.html',
  imports: [
    TaskTabsComponent,
    PageHeaderComponent,
    TranslateModule,
    NgbTooltipModule,
    CommonModule
  ]
})
export class TaskDetailsShellComponent implements OnInit {

  @Input() taskId!: number;
  @Input() createdByMe: boolean = false;
  requireUploadFile = false;
 
  readonly = false;
  showAdminPages = true;

  constructor(
    private route: ActivatedRoute, private modal: NgbModal, private refreshService: TaskDetailsRefreshService,
    private taskService: TaskService,    public modall: NgbActiveModal,private auth: AuthService
     ) { }

  
  taskInfo: TaskGet | null = null;

 ngOnInit(): void {
  this.showAdminPages = this.createdByMe; 
  console.log(this.showAdminPages);

  if (this.taskId) {
    this.loadTask();
  }
}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['taskId'] && this.taskId) {
      this.loadTask();
    }
  }


 loadTask() {
    this.taskService.getById(this.taskId).subscribe({
      next: res => {
        this.taskInfo = res.data;
        this.requireUploadFile = this.taskInfo.requireUploadFile;
        console.log(`dddd: ${this.createdByMe}`)
      },
      error: err => console.error('Failed to load task', err)
    });

    
  }

get isTaskClosed(): boolean {
  const status = this.taskInfo?.status;
  return status === 5
      || status === 4
      || status === 3;
}

  openAction(action: string) {
    if (this.isTaskClosed) return;

    if (action === 'comment') {
      const ref = this.modal.open(CommentModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;
      ref.componentInstance.requireUploadFile = this.taskInfo?.requireUploadFile ?? false;

      ref.result.then(
        (success) => {
          if (success) {
            this.refreshService.trigger('comment');
          }
        },
        () => { }
      );
    }
    if (action === 'warning') {
      const ref = this.modal.open(WarningModalComponent, { size: 'lg' });
      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        ok => ok && this.refreshService.trigger('warning'),
        () => { }
      );
    } 



     if (action === 'percent') {
      const ref = this.modal.open(PercentageModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        (success) => {
          if (success) {
            this.refreshService.trigger('percent');
          }
        },
        () => { }
      );
    }

    if (action === 'penalty') {
      const ref = this.modal.open(PenaltyModalComponent, { size: 'lg' });
      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        ok => ok && this.refreshService.trigger('penalty'),
        () => { }
      );
    }
    if (action === 'extend') {
    const ref = this.modal.open(ExtendRequestComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    ref.componentInstance.taskId = this.taskId;
    return;
  }
    if (action === 'close') {
    const ref = this.modal.open(CloseRequestModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    ref.componentInstance.taskId = this.taskId;

    ref.result.then(
      success => {
        if (success) {
          this.refreshService.trigger('close-request');
        }
      },
      () => {}
    );
  }
  }
}
