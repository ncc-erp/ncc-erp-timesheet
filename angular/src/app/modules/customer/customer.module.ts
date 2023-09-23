import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule, NgForm, FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerRoutingModule } from './customer-routing.module';
import { CustomerComponent } from './customer.component';
import { SharedModule } from '@shared/shared.module';
import { CreateEditCustomerComponent } from './create-edit-customer/create-edit-customer.component';
// import { LocalizePipe } from '@shared/pipes/localize.pipe';

@NgModule({
  declarations: [CustomerComponent, CreateEditCustomerComponent],
  imports: [
    CommonModule,
    CustomerRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule
    // LocalizePipe
  ],
  entryComponents: [
    CreateEditCustomerComponent
  ],
  exports: [
    CreateEditCustomerComponent
  ]
})
export class CustomerModule { }
