import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { PayRunResponse } from '../../models/payrun.model';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  private http = inject(HttpClient);

  payruns = signal<PayRunResponse[]>([]);
  showModal = signal(false);
  period = signal('');
  grossPay = signal(0);
  ytdGross = signal(0);
  selfEmployed = signal(false);
  submitting = signal(false);

  openModal() { this.showModal.set(true); }

  closeModal() {
    this.showModal.set(false);
    this.period.set('');
    this.grossPay.set(0);
    this.ytdGross.set(0);
    this.selfEmployed.set(false);
  }

  submit() {
    if (!this.period() || this.grossPay() <= 0) return;
    const [year, month] = this.period().split('-').map(Number);
    const pad = (n: number) => String(n).padStart(2, '0');
    const periodStart = `${year}-${pad(month)}-01`;
    const periodEnd = `${year}-${pad(month)}-${new Date(year, month, 0).getDate()}`;

    this.submitting.set(true);
    this.http.post<PayRunResponse>('/api/payruns', {
      employeeId: 1,
      periodStart,
      periodEnd,
      grossPay: this.grossPay(),
      frequency: 12,
      selfEmployed: this.selfEmployed(),
      ytdGrossEarnings: this.ytdGross(),
      ytdQppTier1: 0,
      ytdQppTier2: 0,
      ytdEiPremiums: 0,
      ytdQpipPremiums: 0,
    }).subscribe({
      next: (result) => {
        this.payruns.update(prev => [result, ...prev]);
        this.submitting.set(false);
        this.closeModal();
      },
      error: () => { this.submitting.set(false); }
    });
  }
}
