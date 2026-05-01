import { Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AdminTableComponent } from '../../../shared/components/admin-table/admin-table.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-admin-artists',
  standalone: true,
  imports: [AdminTableComponent, ConfirmDialogComponent],
  templateUrl: './artists.component.html',
})
export class ArtistsComponent implements OnInit {
  private http = inject(HttpClient);
  baseUrl = 'http://localhost:8000/api/artists';
  page = 1; pageSize = 10;
  artists: any[] = [];
  totalCount = 0; totalPages = 1;
  isLoading = true;
  deleteTarget: any = null;
  isDeleting = false;
  message: { type: string; text: string } | null = null;

  columns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'genre', label: 'Genre', sortable: true },
    { key: 'biography', label: 'Biography', render: (item: any) => item.biography ? item.biography.substring(0, 60) + (item.biography.length > 60 ? '...' : '') : '-' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ];

  keyExtractor = (item: any) => item.id;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    this.http.get<any>(`${this.baseUrl}?page=${this.page}&pageSize=${this.pageSize}`)
      .subscribe({
        next: (r) => { this.artists = r.data; this.totalCount = r.totalCount; this.totalPages = r.totalPages; this.isLoading = false; },
        error: () => { this.message = { type: 'error', text: 'Failed to load artists' }; this.isLoading = false; },
      });
  }

  onPageChange(page: number): void { this.page = page; this.load(); }

  onEdit(artist: any): void {
    window.location.href = '/music/artists';
  }

  onDelete(artist: any): void { this.deleteTarget = artist; }

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
