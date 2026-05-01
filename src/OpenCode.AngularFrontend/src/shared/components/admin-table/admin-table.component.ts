import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../pagination/pagination';

export interface AdminColumn {
  key: string;
  label: string;
  sortable?: boolean;
  render?: (item: any) => string;
}

@Component({
  selector: 'app-admin-table',
  standalone: true,
  imports: [FormsModule, PaginationComponent],
  templateUrl: './admin-table.component.html',
  styleUrl: './admin-table.component.css',
})
export class AdminTableComponent {
  @Input() columns: AdminColumn[] = [];
  @Input() data: any[] = [];
  @Input() keyExtractor: (item: any) => string | number = () => 0;
  @Input() isLoading = false;
  @Input() page = 1;
  @Input() totalPages = 1;
  @Input() totalCount = 0;
  @Output() edit = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<number>();

  search = '';
  sortKey: string | null = null;
  sortDir: 'asc' | 'desc' = 'asc';

  get filteredData(): any[] {
    if (!this.search) return this.data;
    const q = this.search.toLowerCase();
    return this.data.filter((item: any) =>
      this.columns.some(col => {
        const val = item[col.key];
        return val != null && String(val).toLowerCase().includes(q);
      })
    );
  }

  get sortedData(): any[] {
    if (!this.sortKey) return this.filteredData;
    return [...this.filteredData].sort((a: any, b: any) => {
      const aVal = a[this.sortKey!];
      const bVal = b[this.sortKey!];
      if (aVal == null) return 1;
      if (bVal == null) return -1;
      const cmp = String(aVal).localeCompare(String(bVal));
      return this.sortDir === 'asc' ? cmp : -cmp;
    });
  }

  toggleSort(key: string): void {
    if (this.sortKey === key) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortKey = key;
      this.sortDir = 'asc';
    }
  }
}
