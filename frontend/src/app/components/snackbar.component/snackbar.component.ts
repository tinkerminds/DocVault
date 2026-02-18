import {
    Component,
    OnInit,
    OnDestroy,
    ChangeDetectorRef,
    ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { SnackbarService, SnackbarMessage } from '../../services/snackbar.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface ActiveSnackbar extends SnackbarMessage {
    id: number;
    visible: boolean;
}

@Component({
    selector: 'app-snackbar',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './snackbar.component.html',
    styleUrl: './snackbar.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SnackbarComponent implements OnInit, OnDestroy {
    snackbars: ActiveSnackbar[] = [];
    private idCounter = 0;
    private destroy$ = new Subject<void>();

    constructor(
        private snackbarService: SnackbarService,
        private cdr: ChangeDetectorRef,
    ) { }

    ngOnInit(): void {
        this.snackbarService.messages$.pipe(takeUntil(this.destroy$)).subscribe((msg) => {
            this.addSnackbar(msg);
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    dismiss(id: number): void {
        this.startExit(id);
    }

    getIcon(type: string): string {
        switch (type) {
            case 'success': return 'check_circle';
            case 'error': return 'error';
            default: return 'info';
        }
    }

    trackById(_index: number, snack: ActiveSnackbar): number {
        return snack.id;
    }

    private addSnackbar(msg: SnackbarMessage): void {
        const id = ++this.idCounter;
        const snackbar: ActiveSnackbar = { ...msg, id, visible: false };
        this.snackbars.push(snackbar);
        this.cdr.detectChanges();

        // Trigger enter animation on next paint
        requestAnimationFrame(() => {
            snackbar.visible = true;
            this.cdr.detectChanges();
        });

        // Auto-dismiss after duration
        setTimeout(() => {
            this.startExit(id);
        }, msg.duration ?? 3000);
    }

    private startExit(id: number): void {
        const snackbar = this.snackbars.find((s) => s.id === id);
        if (!snackbar || !snackbar.visible) return;

        snackbar.visible = false;
        this.cdr.detectChanges();

        // Remove from DOM after CSS transition completes (400ms)
        setTimeout(() => {
            this.snackbars = this.snackbars.filter((s) => s.id !== id);
            this.cdr.detectChanges();
        }, 400);
    }
}
