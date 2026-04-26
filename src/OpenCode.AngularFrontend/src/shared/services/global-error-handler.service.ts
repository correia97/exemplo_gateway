import { ErrorHandler, Injectable, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private toastService = inject(ToastService);

  handleError(error: any): void {
    console.error('GlobalErrorHandler caught:', error);

    let message = 'An unexpected error occurred';
    if (error instanceof Error) {
      message = error.message || message;
    } else if (typeof error === 'string') {
      message = error;
    } else if (error?.message) {
      message = error.message;
    }

    this.toastService.showError(message);
  }
}
