import { Component, effect, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideFileText, LucideMenu, LucideLogIn, LucideLogOut, LucideSun, LucideMoon } from '@lucide/angular';
import { Modal } from './components/modal/modal';
import { AuthService } from './services/auth.service';
import { I18nService } from './services/i18n.service';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FormsModule, LucideFileText, LucideMenu, LucideLogIn, LucideLogOut, LucideSun, LucideMoon, Modal],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  auth = inject(AuthService);
  i18n = inject(I18nService);
  theme = inject(ThemeService);
  protected t = this.i18n.t;

  showLoginModal = signal(false);
  loginError = signal(false);
  creds = { username: '', password: '' };

  constructor() {
    effect(() => { document.documentElement.lang = this.i18n.lang(); });
    effect(() => { document.documentElement.setAttribute('data-theme', this.theme.theme()); });
  }

  openLogin() {
    this.loginError.set(false);
    this.creds = { username: '', password: '' };
    this.showLoginModal.set(true);
  }

  closeLogin() { this.showLoginModal.set(false); }

  login() {
    this.auth.login(this.creds.username, this.creds.password).subscribe({
      next: () => this.closeLogin(),
      error: () => this.loginError.set(true),
    });
  }

  logout() { this.auth.logout(); }
}
