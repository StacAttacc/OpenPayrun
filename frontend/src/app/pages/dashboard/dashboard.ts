import { Component, inject } from '@angular/core';
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

  payruns: PayRunResponse[] = [];
  showModal = false;
  period = '';
  grossPay = 0;
  selfEmployed = false;
  submitting = false;

  openModal() { this.showModal = true; }

  closeModal() {
    this.showModal = false;
    this.period = '';
    this.grossPay = 0;
    this.selfEmployed = false;
  }

  submit() {
    if (!this.period || this.grossPay <= 0) return;
    const [year, month] = this.period.split('-').map(Number);
    const pad = (n: number) => String(n).padStart(2, '0');
    const periodStart = `${year}-${pad(month)}-01`;
    const periodEnd = `${year}-${pad(month)}-${new Date(year, month, 0).getDate()}`;

    this.submitting = true;
    this.http.post<PayRunResponse>('/api/payruns', {
      employeeId: 1,
      periodStart,
      periodEnd,
      grossPay: this.grossPay,
      frequency: 12,
      selfEmployed: this.selfEmployed,
      ytdGrossEarnings: 0,
      ytdQppTier1: 0,
      ytdQppTier2: 0,
      ytdEiPremiums: 0,
      ytdQpipPremiums: 0,
    }).subscribe({
      next: (result) => {
        this.payruns = [result, ...this.payruns];
        this.submitting = false;
        this.closeModal();
      },
      error: () => { this.submitting = false; }
    });
  }
}
