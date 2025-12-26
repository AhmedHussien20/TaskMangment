import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { BREADCRUMB_ROUTES } from 'app/shared/routes/breadcrumb.routes';

@Component({
  selector: 'app-page-header', 
  imports: [CommonModule, TranslateModule],
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.scss']
})
export class PageHeaderComponent {
  @Input() title: string = '';
  @Input() title1: string[] = [];  
  @Input() activeitem: string = '';  
  crumbRoutes = BREADCRUMB_ROUTES;

  constructor(private router: Router) {}

  navigate(crumb: string) {
    const route = this.crumbRoutes[crumb];
    if (route) {
      this.router.navigate([route]);
    }
  }
}
