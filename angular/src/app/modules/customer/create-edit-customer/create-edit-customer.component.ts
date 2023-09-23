import { AppComponentBase } from '@shared/app-component-base';
import { CustomerService } from '../../../service/api/customer.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Component, OnInit, Optional, Injector, Inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { CustomerDto } from '../customer.component';

@Component({
  selector: 'app-create-edit-customer',
  templateUrl: './create-edit-customer.component.html',
  styleUrls: ['./create-edit-customer.component.css']
})
export class CreateEditCustomerComponent extends AppComponentBase implements OnInit  {
  customer = {} as CustomerDto;
  title: string;
  active: boolean = true;
  respone = 0;
  saving: boolean = false;
  isSaving: boolean = false;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    injector: Injector,
    private customerService: CustomerService,
    private _dialogRef: MatDialogRef<CreateEditCustomerComponent>,
  ) {
    super(injector);
   } 

   ngOnInit() {
    this.customer = this.data;
    this.title = this.customer.id != null ? 'Edit Client: ' : 'New Client';
  }

  save() {
    this.isSaving = true;
    this.customerService.save(this.customer).subscribe(res => {
      if (res.success == true) {
        if (this.customer.id == null) {
          this.notify.success(this.l('Create Client Successfully'));
        }
        else {
          this.notify.success(this.l('Update Client Successfully'));
        }
        this.respone = 1;
        this.close(res.result);
      }
    }, err => {
      this.isSaving = false;
    })
  }

  close(res): void {
    this._dialogRef.close(res);
  }

  
}
