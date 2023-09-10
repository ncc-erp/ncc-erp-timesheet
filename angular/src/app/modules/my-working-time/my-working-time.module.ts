import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { FormsModule } from '@angular/forms';
import { MyWorkingTimeComponent } from './my-working-time.component';
import { MyWorkingTimeRoutingModule } from './my-working-time-routing.module';
import { RegisterWorkingTimeComponent } from './register-working-time/register-working-time.component';
@NgModule({
  declarations: [MyWorkingTimeComponent, RegisterWorkingTimeComponent],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    MyWorkingTimeRoutingModule,
    
  ],
  entryComponents: [
    RegisterWorkingTimeComponent
  ]
})
export class MyWorkingTimeModule { }
