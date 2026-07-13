import { Component } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employees',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './employees.html',
  styleUrl: './employees.css',
})
export class Employees {
  employees: Employee[] = [
    { id: 1, firstName: 'Alice', lastName: 'Martin', hourlyRate: 28.50, startDate: '2023-03-15' },
    { id: 2, firstName: 'Bob', lastName: 'Tremblay', hourlyRate: 32.00, startDate: '2022-09-01' }
  ];
}
