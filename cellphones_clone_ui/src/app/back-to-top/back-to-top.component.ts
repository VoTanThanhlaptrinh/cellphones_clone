import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-back-to-top',
  imports: [],
  standalone: true,
  templateUrl: './back-to-top.component.html',
  styleUrl: './back-to-top.component.css'
})
export class BackToTopComponent {
  @Output() scrollToTop = new EventEmitter<void>();

  clickToTop(): void {

    this.scrollToTop.emit();
  }
}
