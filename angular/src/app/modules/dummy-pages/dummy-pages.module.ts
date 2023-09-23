import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmMailComponent } from './confirm-mail/confirm-mail.component';
import { ComplainMailComponent } from './complain-mail/complain-mail.component';
import { DummyPagesComponent } from './dummy-pages.component';
import { DummyPageRoutingModule } from './dummy-pages-routing.module';
import { SharedModule } from '@shared/shared.module';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [ConfirmMailComponent, ComplainMailComponent, DummyPagesComponent],
  imports: [
    CommonModule,
    DummyPageRoutingModule,
    SharedModule,
    FormsModule
  ]
})
export class DummyPagesModule { }
