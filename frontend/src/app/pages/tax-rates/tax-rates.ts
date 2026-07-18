import { Component, computed, inject, signal, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DecimalPipe, DatePipe, PercentPipe, CurrencyPipe } from '@angular/common';
import { injectQuery, injectMutation, injectQueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { Modal } from '../../components/modal/modal';
import { TaxRateSet, TaxRateSetBody } from '../../models/tax-rate.model';
import { CalcResult } from '../../models/pay-run.model';
import { AuthService } from '../../services/auth.service';
import { I18nService } from '../../services/i18n.service';
import { ScrollFadeDirective } from '../../directives/scroll-fade.directive';

interface CalcPayload {
  periodStart: string;
  grossPay: number;
  frequency: number;
  selfEmployed: boolean;
  ytdGrossEarnings: number;
}

@Component({
  selector: 'app-tax-rates',
  imports: [FormsModule, DecimalPipe, DatePipe, PercentPipe, CurrencyPipe, Modal, ScrollFadeDirective],
  templateUrl: './tax-rates.html',
  styleUrl: './tax-rates.css',
})
export class TaxRates {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);
  private queryClient = injectQueryClient();
  authService = inject(AuthService);
  protected t = inject(I18nService).t;

  page = signal(1);
  readonly pageSize = 14;

  showModal = signal(false);
  editingId = signal<number | null>(null);
  confirmingDelete = signal(false);
  draft: TaxRateSetBody = emptyDraft();

  showCalcModal = signal(false);
  calcResult = signal<CalcResult | null>(null);
  calc = {
    period: new Date().toISOString().slice(0, 7),
    grossPay: 0,
    ytdGrossEarnings: 0,
    selfEmployed: false,
  };

  taxRatesQuery = injectQuery(() => ({
    queryKey: ['tax-rates'],
    queryFn: () => firstValueFrom(
      this.http.get<TaxRateSet[]>('/api/tax-rates').pipe(
        map(d => [...d].sort((a, b) => b.effectiveFrom.localeCompare(a.effectiveFrom)))
      )
    ),
  }));

  rates = computed(() => this.taxRatesQuery.data() ?? []);
  pagedRates = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.rates().slice(start, start + this.pageSize);
  });
  totalPages = computed(() => Math.max(1, Math.ceil(this.rates().length / this.pageSize)));

  prevPage() { if (this.page() > 1) this.page.update(p => p - 1); }
  nextPage() { if (this.page() < this.totalPages()) this.page.update(p => p + 1); }

  saveMutation = injectMutation(() => ({
    mutationFn: ({ id, body }: { id: number | null; body: TaxRateSetBody }) =>
      id
        ? firstValueFrom(this.http.put<TaxRateSet>(`/api/tax-rates/${id}`, body))
        : firstValueFrom(this.http.post<TaxRateSet>('/api/tax-rates', body)),
    onSuccess: () => {
      this.queryClient.invalidateQueries({ queryKey: ['tax-rates'] });
      this.close();
    },
  }));

  deleteMutation = injectMutation(() => ({
    mutationFn: (id: number) => firstValueFrom(this.http.delete(`/api/tax-rates/${id}`)),
    onSuccess: () => {
      this.queryClient.invalidateQueries({ queryKey: ['tax-rates'] });
      this.close();
    },
  }));

  calcMutation = injectMutation(() => ({
    mutationFn: (payload: CalcPayload) =>
      firstValueFrom(this.http.post<CalcResult>('/api/pay-runs/calculate', payload)),
    onSuccess: result => this.calcResult.set(result),
  }));

  submitting = computed(() => this.saveMutation.isPending() || this.deleteMutation.isPending());
  calculating = computed(() => this.calcMutation.isPending());

  openAdd() {
    const latest = this.rates()[0];
    this.draft = latest
      ? { ...latest, effectiveFrom: '', effectiveTo: null,
          federalBrackets: latest.federalBrackets.map(b => ({ ...b })),
          quebecBrackets: latest.quebecBrackets.map(b => ({ ...b })) }
      : emptyDraft();
    this.editingId.set(null);
    this.showModal.set(true);
  }

  openEdit(r: TaxRateSet) {
    this.draft = { ...r,
      federalBrackets: r.federalBrackets.map(b => ({ ...b })),
      quebecBrackets: r.quebecBrackets.map(b => ({ ...b })) };
    this.editingId.set(r.id);
    this.showModal.set(true);
  }

  close() { this.showModal.set(false); this.confirmingDelete.set(false); }

  save() {
    const body = { ...this.draft, effectiveTo: this.draft.effectiveTo || null };
    this.saveMutation.mutate({ id: this.editingId(), body });
  }

  delete() {
    if (!this.confirmingDelete()) { this.confirmingDelete.set(true); return; }
    this.deleteMutation.mutate(this.editingId()!);
  }

  openCalc() {
    this.calcResult.set(null);
    this.showCalcModal.set(true);
  }

  closeCalc() { this.showCalcModal.set(false); }

  calculate() {
    const periodStart = this.calc.period + '-01';
    this.calcMutation.mutate({
      periodStart,
      grossPay: this.calc.grossPay,
      frequency: this.deriveFrequency(),
      selfEmployed: this.calc.selfEmployed,
      ytdGrossEarnings: this.calc.ytdGrossEarnings,
    });
  }

  private deriveFrequency(): number {
    const { grossPay, ytdGrossEarnings, period } = this.calc;
    const month = parseInt(period.split('-')[1], 10);
    if (!grossPay || !ytdGrossEarnings) return 12;
    const priorPeriods = Math.round(ytdGrossEarnings / grossPay);
    const impliedFrequency = (priorPeriods + 1) / (month / 12);
    const valid = [12, 24, 26, 52];
    return valid.reduce((a, b) =>
      Math.abs(b - impliedFrequency) < Math.abs(a - impliedFrequency) ? b : a
    );
  }

  addFederalBracket() {
    this.draft.federalBrackets = [...this.draft.federalBrackets, { upperBound: null, rate: 0 }];
    this.cdr.markForCheck();
  }
  removeFederalBracket(i: number) {
    this.draft.federalBrackets = this.draft.federalBrackets.filter((_, j) => j !== i);
    this.cdr.markForCheck();
  }
  addQuebecBracket() {
    this.draft.quebecBrackets = [...this.draft.quebecBrackets, { upperBound: null, rate: 0 }];
    this.cdr.markForCheck();
  }
  removeQuebecBracket(i: number) {
    this.draft.quebecBrackets = this.draft.quebecBrackets.filter((_, j) => j !== i);
    this.cdr.markForCheck();
  }
}

function emptyDraft(): TaxRateSetBody {
  return {
    effectiveFrom: '', effectiveTo: null,
    qppExemption: 0, qppYmpe: 0, qppYampe: 0,
    qppBaseRate: 0, qppAdditionalTier1Rate: 0, qppTier1Rate: 0, qppTier2Rate: 0,
    qppTier1MaxEmployee: 0, qppTier2MaxEmployee: 0,
    eiEmployeeRate: 0, eiEmployerMultiplier: 0, eiMaxInsurableEarnings: 0, eiMaxEmployeePremium: 0,
    qpipEmployeeRate: 0, qpipEmployerRate: 0, qpipMaxInsurableEarnings: 0,
    qpipMaxEmployeePremium: 0, qpipMaxEmployerPremium: 0,
    federalBasicPersonalAmount: 0, federalEmploymentAmount: 0,
    federalLowestRate: 0, quebecFederalAbatement: 0,
    quebecBasicPersonalAmount: 0, quebecWorkerDeductionMax: 0,
    quebecWorkerDeductionRate: 0, quebecLowestRate: 0,
    fssqSmallEmployerRate: 0, fssqLargeEmployerRate: 0,
    federalBrackets: [], quebecBrackets: [],
  };
}
