import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OffDayProjectDetailComponent } from './off-day-project-detail.component';
import { SharedModule } from '@shared/shared.module';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [OffDayProjectDetailComponent],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule,
  ]
})
export class OffDayProjectDetailModule { }
