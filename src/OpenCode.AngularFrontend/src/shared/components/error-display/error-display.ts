import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-error-display',
  standalone: true,
  templateUrl: './error-display.html',
  styleUrl: './error-display.css',
})
export class ErrorDisplayComponent implements OnInit, OnDestroy {
  private toastService = inject(ToastService);
  private subscription?: Subscription;

  toasts: Toast[] = [];
  copiedId: number | null = null;

  ngOnInit(): void {
    this.subscription = this.toastService.toasts$.subscribe(toast => {
      if ((toast as any)._dismiss) {
        this.toasts = this.toasts.filter(t => t.id !== toast.id);
        if (this.copiedId === toast.id) this.copiedId = null;
      } else {
        this.toasts = [...this.toasts, toast];
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  dismiss(id: number): void {
    this.toastService.dismiss(id);
  }

  copyCorrelationId(id: number, correlationId: string): void {
    navigator.clipboard.writeText(correlationId).then(() => {
      this.copiedId = id;
      setTimeout(() => {
        if (this.copiedId === id) this.copiedId = null;
      }, 2000);
    }).catch(() => {});
  }
}
