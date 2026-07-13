import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Employees } from './pages/employees/employees';

export const routes: Routes = [
  { path: '', component: Dashboard },
  { path: 'employees', component: Employees },
  { path: '**', redirectTo: '' }
];
