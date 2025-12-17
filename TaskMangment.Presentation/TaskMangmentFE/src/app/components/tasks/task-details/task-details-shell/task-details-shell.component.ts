import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TaskTabsComponent } from '../task-tabs/task-tabs.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { CommentModalComponent } from '../actions/comment/comment-modal/comment-modal.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TaskDetailsRefreshService } from '../task-details-refresh.service';
import { WarningModalComponent } from '../actions/warning/warning-modal/warning-modal.component';
import { PenaltyModalComponent } from '../actions/penalty/penalty-modal/penalty-modal.component';
import { CloseRequestModalComponent } from '../actions/close-request/close-request-modal/close-request-modal.component';
import { ExtendRequestComponent } from '../actions/extend-request/extend-request.component';

@Component({
  selector: 'app-task-details-shell',
  standalone: true,
  templateUrl: './task-details-shell.component.html',
  imports: [
    TaskTabsComponent,
    PageHeaderComponent,
    TranslateModule
  ]
})
export class TaskDetailsShellComponent implements OnInit {

  taskId!: number;
  readonly = false;

  constructor(private route: ActivatedRoute, private modal: NgbModal, private refreshService: TaskDetailsRefreshService) { }

  ngOnInit(): void {
    this.taskId = Number(this.route.snapshot.paramMap.get('id'));
  }

  openAction(action: string) {

    if (action === 'comment') {
      const ref = this.modal.open(CommentModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;

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
