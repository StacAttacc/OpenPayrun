import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  theme = signal<Theme>((localStorage.getItem('theme') as Theme) ?? 'light');

  toggle() {
    const next: Theme = this.theme() === 'light' ? 'dark' : 'light';
    localStorage.setItem('theme', next);
    this.theme.set(next);
  }
}
