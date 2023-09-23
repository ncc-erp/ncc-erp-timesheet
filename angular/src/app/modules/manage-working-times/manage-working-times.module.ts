import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@node_modules/@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ManageWorkingTimesRoutingModule } from './manage-working-times-routing.module';
import { ManageWorkingTimesComponent } from './manage-working-times.component';

@NgModule({
  declarations: [ManageWorkingTimesComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    ManageWorkingTimesRoutingModule
  ]
})
export class ManageWorkingTimesModule { }
