import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export type SnackbarType = 'success' | 'error' | 'info';

export interface SnackbarMessage {
    message: string;
    type: SnackbarType;
    duration?: number; // ms, default 3000
}

@Injectable({ providedIn: 'root' })
export class SnackbarService {
    private _messages$ = new Subject<SnackbarMessage>();
    readonly messages$ = this._messages$.asObservable();

    show(message: string, type: SnackbarType = 'info', duration = 3000): void {
        this._messages$.next({ message, type, duration });
    }

    success(message: string, duration = 3000): void {
        this.show(message, 'success', duration);
    }

    error(message: string, duration = 4000): void {
        this.show(message, 'error', duration);
    }

    info(message: string, duration = 3000): void {
        this.show(message, 'info', duration);
    }
}
