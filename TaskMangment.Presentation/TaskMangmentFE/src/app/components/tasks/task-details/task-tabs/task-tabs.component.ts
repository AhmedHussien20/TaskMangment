import { Component, Input } from '@angular/core';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { TaskEmployeesComponent } from '../tabs/employees/task-employees/task-employees.component';
import { TaskBasicInfoComponent } from '../tabs/basic-info/task-basic-info/task-basic-info.component';
import { TranslateModule } from '@ngx-translate/core';
import { TaskWarningsComponent } from '../tabs/warnings/task-warnings/task-warnings.component';
import { TaskPenaltiesComponent } from '../tabs/penalties/task-penalties/task-penalties.component';
import { TaskAuditComponent } from '../tabs/audit/task-audit/task-audit.component';
import { TaskRequestsComponent } from '../tabs/requests/task-requests/task-requests.component';
import { TaskCommentsComponent } from '../tabs/comments/task-comments/task-comments.component';

@Component({
  selector: 'app-task-tabs',
  standalone: true,
  imports: [NgbNavModule,TaskEmployeesComponent,TaskBasicInfoComponent ,TaskWarningsComponent, TranslateModule,TaskPenaltiesComponent,TaskAuditComponent,TaskBasicInfoComponent,TaskRequestsComponent,TaskCommentsComponent ],
  templateUrl: './task-tabs.component.html'
})
export class TaskTabsComponent {

  @Input() taskId!: number;
  @Input() createdByMe: boolean = false;
  @Input() readonly = false;
  @Input() canSendPenalty = false;
  @Input() canSendWarning = false;

  activeTab = 'basic';
}