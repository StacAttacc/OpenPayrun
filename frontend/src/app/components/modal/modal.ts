import { Component, input, output } from '@angular/core';
import { ScrollFadeDirective } from '../../directives/scroll-fade.directive';

@Component({
  selector: 'app-modal',
  imports: [ScrollFadeDirective],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
})
export class Modal {
  title = input.required<string>();
  show = input.required<boolean>();
  closed = output<void>();
}
