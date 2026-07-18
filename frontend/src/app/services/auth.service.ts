import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private token = signal<string | null>(sessionStorage.getItem('token'));

  readonly isAdmin = computed(() => this.token() !== null);

  login(username: string, password: string) {
    return this.http.post<{ token: string }>('/api/auth/login', { username, password }).pipe(
      tap(res => {
        sessionStorage.setItem('token', res.token);
        this.token.set(res.token);
      })
    );
  }

  logout() {
    sessionStorage.removeItem('token');
    this.token.set(null);
  }

  getToken() { return this.token(); }
}
