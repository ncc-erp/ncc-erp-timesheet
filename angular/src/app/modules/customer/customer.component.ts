import { CreateEditCustomerComponent } from './create-edit-customer/create-edit-customer.component';
import { MatDialog, PageEvent } from '@angular/material';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CustomerService } from './../../service/api/customer.service';
import { Component, OnInit, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { finalize } from 'rxjs/operators';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';


@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.css'],
  animations: [appModuleAnimation()]
})

export class CustomerComponent extends PagedListingComponentBase<CustomerDto> implements OnInit {
  ADD_CLIENT = PERMISSIONS_CONSTANT.AddClient;
  EDIT_CLIENT = PERMISSIONS_CONSTANT.EditClient;
  DELETE_CLIENT = PERMISSIONS_CONSTANT.DeleteClient;

  isActive: boolean | null;

  customers = [] as CustomerDto[];


  constructor(
    private customerService: CustomerService,
    private dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.customerService
      .getAllPagging(request)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.customers = result.result.items;
        this.showPaging(result.result, pageNumber);
      });
  }

  protected delete(item: CustomerDto): void {
    abp.message.confirm(
      "Delete customer '" + item.name + "'?",
      (result: boolean) => {
        if (result) {
          this.customerService.delete(item.id).subscribe(() => {
            abp.notify.info('Deleted customer: ' + item.name);
            this.refresh();
          });
        }
      }
    );
  }

  createCustomer(): void {
    let customer = {} as CustomerDto;
    this.showDialog(customer);
  }

  editCustomer(customer: CustomerDto): void {
    this.showDialog(customer);
  }


  showDialog(customer: CustomerDto): void {
    let item = { id: customer.id, name: customer.name, code: customer.code, address: customer.address } as CustomerDto;
    const dialogRef = this.dialog.open(CreateEditCustomerComponent, {
      data: item,
      disableClose : true
    });

    dialogRef.afterClosed().subscribe((res) => {
      if(res) {
        if (customer.id == null) {
          this.getDataPage(1);
        }
        else {
          this.getDataPage(this.pageNumber);
        }
      }
    });
  }
}

export class CustomerDto {
  id: number;
  name: string;
  code: string;
  address: string;
}
