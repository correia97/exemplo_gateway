import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export interface Toast {
  id: number;
  message: string;
  correlationId?: string;
  type: 'error' | 'success' | 'info';
  createdAt: number;
  _dismiss?: boolean;
}

const TOAST_DURATION = 8000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toastSubject = new Subject<Toast>();
  private nextId = 0;
  private timers = new Map<number, ReturnType<typeof setTimeout>>();

  get toasts$(): Observable<Toast> {
    return this.toastSubject.asObservable();
  }

  showError(message: string, correlationId?: string): number {
    return this.addToast({ message, correlationId, type: 'error' });
  }

  showSuccess(message: string): number {
    return this.addToast({ message, type: 'success' });
  }

  showInfo(message: string): number {
    return this.addToast({ message, type: 'info' });
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.toastSubject.next({ id, message: '', type: 'info', createdAt: 0, _dismiss: true } as any);
  }

  dismissAll(): void {
    this.timers.forEach(timer => clearTimeout(timer));
    this.timers.clear();
  }

  private addToast(data: { message: string; correlationId?: string; type: Toast['type'] }): number {
    const id = this.nextId++;
    const toast: Toast = {
      id,
      message: data.message,
      correlationId: data.correlationId,
      type: data.type,
      createdAt: Date.now(),
    };
    this.toastSubject.next(toast);
    const timer = setTimeout(() => this.dismiss(id), TOAST_DURATION);
    this.timers.set(id, timer);
    return id;
  }
}
