import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PopupComponent } from './popup.component';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [PopupComponent],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule,
  ]
})
export class PopupModule { }
