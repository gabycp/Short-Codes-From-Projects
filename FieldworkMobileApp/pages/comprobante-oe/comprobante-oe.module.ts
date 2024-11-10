import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IonicModule } from '@ionic/angular';

import { ComprobanteOEPageRoutingModule } from './comprobante-oe-routing.module';

import { ComprobanteOEPage } from './comprobante-oe.page';
import { PhotoComponent } from '../photo/photo.component';
import { ComponentsModule } from 'src/app/components/components.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    ComprobanteOEPageRoutingModule,
    ComponentsModule
  ],
  declarations: [ComprobanteOEPage]
})
export class ComprobanteOEPageModule {}
