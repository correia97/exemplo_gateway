import { Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AdminTableComponent } from '../../../shared/components/admin-table/admin-table.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-admin-albums',
  standalone: true,
  imports: [AdminTableComponent, ConfirmDialogComponent],
  templateUrl: './albums.component.html',
})
export class AlbumsComponent implements OnInit {
  private http = inject(HttpClient);
  baseUrl = 'http://localhost:8000/api/albums';
  page = 1; pageSize = 10;
  albums: any[] = [];
  totalCount = 0; totalPages = 1;
  isLoading = true;
  deleteTarget: any = null;
  isDeleting = false;
  message: { type: string; text: string } | null = null;

  columns = [
    { key: 'title', label: 'Title', sortable: true },
    { key: 'artistName', label: 'Artist', sortable: true },
    { key: 'releaseYear', label: 'Release Year', sortable: true },
    { key: 'genre', label: 'Genre' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ];

  keyExtractor = (item: any) => item.id;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    this.http.get<any>(`${this.baseUrl}?page=${this.page}&pageSize=${this.pageSize}`)
      .subscribe({
        next: (r) => { this.albums = r.data; this.totalCount = r.totalCount; this.totalPages = r.totalPages; this.isLoading = false; },
        error: () => { this.message = { type: 'error', text: 'Failed to load albums' }; this.isLoading = false; },
      });
  }

  onPageChange(page: number): void { this.page = page; this.load(); }

  onEdit(album: any): void {
    window.location.href = '/music/albums';
  }

  onDelete(album: any): void { this.deleteTarget = album; }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.isDeleting = true;
    this.http.delete(`${this.baseUrl}/${this.deleteTarget.id}`).subscribe({
      next: () => { this.message = { type: 'success', text: 'Deleted successfully' }; this.deleteTarget = null; this.isDeleting = false; this.load(); },
      error: () => { this.message = { type: 'error', text: 'Failed to delete' }; this.isDeleting = false; },
    });
  }

  cancelDelete(): void { this.deleteTarget = null; }
  dismissMessage(): void { this.message = null; }
}
