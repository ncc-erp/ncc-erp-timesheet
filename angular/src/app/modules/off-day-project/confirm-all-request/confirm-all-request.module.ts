import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmAllRequestComponent} from './confirm-all-request.component';
import { SharedModule } from '@shared/shared.module';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [ConfirmAllRequestComponent],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule,
  ]
})
export class ConfirmAllRequestModule { }
