import { Routes } from '@angular/router';
import { AstronautSearchComponent } from './features/components';

export const routes: Routes = [
  {
    path: '',
    component: AstronautSearchComponent,
  },
  {
    path: '**',
    redirectTo: '',
  },
];
