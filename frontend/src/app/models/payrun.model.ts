export interface PayRun {
  id: number;
  employeeId: number;
  periodStart: string;
  periodEnd: string;
  grossPay: number;
  employeeTax: number;
  employerContributions: number;
  netPay: number;
}
