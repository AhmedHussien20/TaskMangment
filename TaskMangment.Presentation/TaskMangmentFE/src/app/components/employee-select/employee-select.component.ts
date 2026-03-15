import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { ToastrService } from 'ngx-toastr';

import { EmployeeService } from 'app/core/services/employee.service';
import { AuthService } from 'app/core/services/auth.service';
export interface EmployeeOption {
  value: number;
  label: string;
  imageUrl?: string | null;
  mobile?: string | null;
  email?: string | null;
}

@Component({
  selector: 'app-employee-ng-select',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, NgSelectModule],
  templateUrl: './employee-select.component.html',
  styleUrls: ['./employee-select.component.scss']
})
export class EmployeeNgSelectComponent implements OnInit {
  @Input() labelKey = 'CHART.EMPLOYEES';
  @Input() placeholderKey = 'FORM.SELECT';
  @Input() disabled = false;
  @Input() clearable = true;
@Input() showLabel: boolean = true;

  @Input() pageSize = 20;

  @Input() selectedEmployeeId?: number;
  @Output() selectedEmployeeIdChange = new EventEmitter<number | undefined>();
  @Input() options: EmployeeOption[] = [];
  isLoading = false;

  private currentTerm = '';
  private currentPage = 1;
  private hasMore = true;
  private canCreateTask = false;


  constructor(
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private authService: AuthService, 

  ) {}

  ngOnInit(): void {
        this.canCreateTask = this.authService.hasPermission('CREATE_TASK');
  }

  onOpen(): void {
    if (this.options.length > 0) return;
    this.search('', 1, true);
  }

  onSearch(term: string): void {
    this.search(term ?? '', 1, true);
  }

  onScrollToEnd(): void {
    if (this.isLoading || !this.hasMore) return;
    this.search(this.currentTerm, this.currentPage + 1, false);
  }

  onValueChange(val: any): void {
    const normalized = (val === null || val === undefined) ? undefined : Number(val);
    this.selectedEmployeeIdChange.emit(
      Number.isFinite(normalized as number) ? (normalized as number) : undefined
    );
  }

  onImageError(ev: Event): void {
    const img = ev.target as HTMLImageElement;
    img.src = 'assets/images/user.png';
  }

  private search(term: string, page: number, reset: boolean): void {
    if (this.isLoading) return;

    this.isLoading = true;

    const request: any = {
      searchKey: term,
      pageIndex: page,
      pageSize: this.pageSize,
      sortColumn: 'Id',
      sortDirection: 'DESC',
    };

    if (this.canCreateTask) {
      request.permissionCode = 'CREATE_TASK';
    }

    this.employeeService.getAll(request).subscribe({
      next: (res) => {
        const rows = res?.data?.data ?? [];

        const mapped: EmployeeOption[] = rows.map((item: any) => ({
          value: item.id,
          label: item.fullName || item.name,
          imageUrl: item.imageUrl,
          mobile: item.mobile,
          email: item.email
        }));

        if (reset) {
          this.options = mapped;
          this.currentTerm = term;
          this.currentPage = 1;
        } else {
          const existing = new Set(this.options.map(x => x.value));
          const merged = mapped.filter(x => !existing.has(x.value));
          this.options = [...this.options, ...merged];
          this.currentPage = page;
        }

        this.hasMore = mapped.length === this.pageSize;

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      }
    });
  }
}
