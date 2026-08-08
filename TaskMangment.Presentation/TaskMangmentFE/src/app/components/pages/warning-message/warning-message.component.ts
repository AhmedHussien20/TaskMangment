import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-warning-message',
  standalone: true,
  imports: [RouterModule, TranslateModule],
  templateUrl: './warning-message.component.html',
  styleUrl: './warning-message.component.scss'
})
export class WarningMessageComponent {
    constructor(private router: Router) {}

 goHome() {
    this.router.navigate(['/dashboard']); 
  }
}
