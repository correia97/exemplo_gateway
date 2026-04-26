import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  templateUrl: './empty-state.html',
})
export class EmptyStateComponent {
  @Input() message = '';
  @Input() actionLabel?: string;
  @Output() action = new EventEmitter<void>();
}
