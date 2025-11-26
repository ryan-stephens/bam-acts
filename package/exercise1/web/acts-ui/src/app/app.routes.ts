import { Routes } from '@angular/router';
import { AstronautSearchComponent } from './shared/components';

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
