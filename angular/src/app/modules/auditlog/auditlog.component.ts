import { Component, Injector, OnInit } from '@angular/core';
import { AuditlogService } from '@app/service/api/auditlog.service';
import { FilterDto, PagedListingComponentBase, PagedRequestDto} from '@shared/paged-listing-component-base';
import * as _ from 'lodash';
import { finalize } from 'rxjs/operators';


@Component({
  selector: 'app-auditlog',
  templateUrl: './auditlog.component.html',
  styleUrls: ['./auditlog.component.css']
})
export class AuditlogComponent extends PagedListingComponentBase<AuditlogDto> implements OnInit {
  auditlogs = [] as AuditlogDto[];
  emailAddressFilter = [];
  emailAddress = [];
  selecteduserId: string = "";
  emailAddressSearch = "";

  iconCondition: string = "transactionDate";
  sortDrirect: number = 0;
  transDate: string = "";
  iconSort: string = "";
  public isLoading:boolean = false;
  public listServiceNames:GetServiceDto[] = [];
  private tempServiceNames:GetServiceDto[] = [];
  public listMethodNames:GetMethodDto[] = [];
  private tempMethodNames:GetMethodDto[] = [];
  public selectedService:string = "";
  public selectedMethod:string = "";
  public searchServiceName:string = "";
  public searchMethodName:string = "";
  constructor(
    private auditlog: AuditlogService,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getEmailAddress();
    this.getAllMethodNameInAuditLog();
    this.getAllServiceNameInAuditLog();
    this.refresh();
  }

  sortOrderBy(data) {
    if (this.iconCondition !== data) {
      this.sortDrirect = -1;
    }
    this.iconCondition = data;
    this.transDate = data;
    this.sortDrirect++;
    if (this.sortDrirect > 1) {
      this.transDate = "";
      this.iconSort = "";
      this.sortDrirect = -1;
    }
    if (this.sortDrirect == 1) {
      this.iconSort = "fas fa-sort-amount-down";
    } else if (this.sortDrirect == 0) {
      this.iconSort = "fas fa-sort-amount-up";
    } else {
      this.iconSort = "fas fa-sort";
    }
    this.refresh();
  }

  getEmailAddress() {
    this.auditlog.getAllEmailAddressInAuditLog().subscribe(data => {
      this.emailAddress  = this.emailAddressFilter = data.result
    })
  }
  getAllServiceNameInAuditLog(){
    this.auditlog.getAllServiceNameInAuditLog().subscribe((data)=>{
      this.listServiceNames = this.tempServiceNames = data.result;
    })
  }
  getAllMethodNameInAuditLog(){
    this.auditlog.getAllMethodNameInAuditLog().subscribe((data)=>{
      this.listMethodNames = this.tempMethodNames = data.result;
    })
  }
  handleSearch() {
    const textSearch = this.emailAddressSearch.toLowerCase().trim();
    if (textSearch) {
      this.emailAddress = this.emailAddressFilter
      .filter(item => item.emailAddress.toLowerCase().trim().includes(textSearch));
    } else {
      this.emailAddress = _.cloneDeep(this.emailAddressFilter);
    }
  }

  handleSearchServiceName() {
    let searchText = this.searchServiceName.toLowerCase().trim();
    if(searchText){
      this.listServiceNames = this.tempServiceNames
      .filter(x=> x.serviceName.toLowerCase().trim().includes(searchText))
    }else{
      this.listServiceNames = this.tempServiceNames;
    }
  }
  handleSearchMethodName() {
    let searchText = this.searchMethodName.toLowerCase().trim();
    if(searchText){
      this.listMethodNames = this.tempMethodNames
      .filter(x=> x.methodName.toLowerCase().trim().includes(searchText))
    }else{
      this.listMethodNames = this.tempMethodNames;
    }
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    let filterItems: FilterDto[] = [];
    request.sort = this.transDate;
    request.sortDirection = this.sortDrirect;
    if(this.selecteduserId &&this.selecteduserId != 'null'){
      filterItems.push({
        comparison: 0,
        propertyName: "userId",
        value: this.selecteduserId
      })
    }
    if(this.selecteduserId == 'null'){
      filterItems.push({
        comparison: 0,
        propertyName: "userId",
        value: null
      })
    }
    if(this.selectedMethod){
      filterItems.push({
        comparison: 0,
        propertyName: "methodName",
        value: this.selectedMethod
      })
    }
    if(this.selectedService){
      filterItems.push({
        comparison: 0,
        propertyName: "serviceName",
        value: this.selectedService
      })
    }

    request.filterItems = filterItems;
    if (this.searchText) {
      request.searchText = this.searchText;
    }
    this.isLoading = true;
    this.auditlog
      .getAll(request)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.auditlogs = result.result.items;
        for (let i = 0; i < this.auditlogs.length; i++) {
          this.auditlogs[i].hideNote = false;
        }
        this.showPaging(result.result, pageNumber);
        this.isLoading = false;
      },()=> this.isLoading = false);

  }

  changeStatusNote(item) {
    item.hideNote = !item.hideNote;
  }

  protected delete(item: AuditlogDto): void {
  }
}
export class AuditlogDto {
  executionDuration: number;
  executionTime: string;
  methodName: string;
  parameters: string;
  serviceName: string;
  emailAddress: string;
  userId: number;
  note: string;
  hideNote: boolean;
}
export class GetServiceDto{
  serviceName: string;
}

export class GetMethodDto{
  methodName: string;
}

