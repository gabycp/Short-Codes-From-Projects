import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ComprobanteOEPage } from './comprobante-oe.page';

const routes: Routes = [
  {
    path: '',
    component: ComprobanteOEPage
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ComprobanteOEPageRoutingModule {}
