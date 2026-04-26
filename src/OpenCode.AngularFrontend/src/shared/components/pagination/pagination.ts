import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.html',
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() totalPages = 1;
  @Output() pageChange = new EventEmitter<number>();

  get hasPrevious(): boolean { return this.page > 1; }
  get hasNext(): boolean { return this.page < this.totalPages; }

  previous(): void {
    if (this.hasPrevious) this.pageChange.emit(this.page - 1);
  }
  next(): void {
    if (this.hasNext) this.pageChange.emit(this.page + 1);
  }
}
