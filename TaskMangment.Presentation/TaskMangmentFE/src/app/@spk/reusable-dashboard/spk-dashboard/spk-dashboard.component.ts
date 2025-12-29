import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'spk-dashboard',
  standalone: true, 
  imports: [  CommonModule,                 // ✅ إجباري لـ *ngIf
    TranslateModule],
  templateUrl: './spk-dashboard.component.html',
  styleUrl: './spk-dashboard.component.scss'
})
export class SpkDashboardComponent {
  @Input() card:any
  constructor(private sanitizer: DomSanitizer ) {}
    sanitizeHtml(html: string): SafeHtml {
      return this.sanitizer.bypassSecurityTrustHtml(html);
    }
    sanitizeIcon(svg: string): SafeHtml {
      return this.sanitizer.bypassSecurityTrustHtml(svg);
    }
}
