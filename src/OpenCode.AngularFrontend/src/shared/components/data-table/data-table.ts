import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NgClass } from '@angular/common';

export interface Column<T> {
  key: string;
  label: string;
  render?: (item: T) => string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [],
  templateUrl: './data-table.html',
  styleUrl: './data-table.css',
})
export class DataTableComponent<T extends { [key: string]: any }> {
  @Input() columns: Column<T>[] = [];
  @Input() data: T[] = [];
  @Input() keyExtractor: (item: T) => string | number = () => 0;
  @Input() isLoading = false;
  @Input() rowClickable = false;
  @Output() rowClick = new EventEmitter<T>();

  trackByKey(index: number): number {
    return index;
  }
}
