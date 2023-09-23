import { ChangeDetectorRef, Component, Injector, OnInit, QueryList, ViewChildren } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { userDTO } from '@app/modules/user/user.component';
import { KumoTrackerDto } from '@app/service/api/model/report-timesheet-Dto';
import { KomuTrackerService } from '@app/service/api/komu-tracker.service';
import { UserService } from '@app/service/api/user.service';
import { AppConsts } from '@shared/AppConsts';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import * as moment from 'moment';
import { finalize } from 'rxjs/operators';
import { SortableModel } from '@shared/sortable/sortable.component';
import { BranchService } from '@app/service/api/branch.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-komu-tracker',
  templateUrl: './komu-tracker.component.html',
  styleUrls: ['./komu-tracker.component.css']
})
export class KomuTrackerComponent extends PagedListingComponentBase<KumoTrackerDto> implements OnInit {
  VIEW_KOMU_TRACKER = PERMISSIONS_CONSTANT.ViewKomuTracker;
  branchId;
  userSearch = new FormControl();
  userControl = new FormControl();
  listUserBase = [];
  listUserFiltered: userDTO[];
  listUser: KumoTrackerDto[] = [];
  name = "";
  emailAddress = "";
  dateAt = "";

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter : BranchDto[]; 
  @ViewChildren('sortThead') private elementRefSortable: QueryList<any>;
  constructor(
    private userService: UserService,
    private kumoTrackerService: KomuTrackerService,
    private ref: ChangeDetectorRef,
    private branchService: BranchService,
    injector: Injector,
  ) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.userSearch.valueChanges.subscribe(() => {
      this.search();
    });
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
  }

  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];

  ngOnInit() {
    this.getData();
    this.getListBranch();
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  changeDate() {
    this.dateAt = moment(this.dateAt).format('YYYY-MM-DD');
    this.refresh();
  }
  public sortable = new SortableModel('workingMinute',1,'DESC')
  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    if(this.sortable.sort){
      request.sort = this.sortable.sort;
      request.sortDirection = this.sortable.sortDirection
    }

    if (this.searchText) {
      request.searchText = this.searchText;
    }
    this.kumoTrackerService.getAllPagging(request, String(this.dateAt), Number(this.branchId), String(this.emailAddress))
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe(resp => {
        this.listUser = resp.result.items;
        this.showPaging(resp.result, pageNumber);
      });
  }
  
  protected delete(): void {}

  formatDate(date: string) {
    return moment(date).format("YYYY-MM-DD");
  }

  getData() {
      this.refresh();
      this.init();
      if(this.permission.isGranted( this.VIEW_KOMU_TRACKER)){
      this.userService.getAllNotPagging().subscribe(result => {
        this.listUserBase = result.result;
        this.listUserFiltered = this.listUserBase;
      });
    }
  }


  init(): void {
    this.listUserFiltered = this.listUserBase.filter(item => item.branchId == this.branchId);
  }

  search(): void {
    var temp = this.userSearch.value.trim().toLowerCase();
    this.listUserFiltered = this.listUserBase.filter(data =>
      data.emailAddress.toLowerCase().includes(temp)
    );
  }
  sortTable(event: any){
    this.sortable = event
    this.changeSortableByName(this.sortable.sort, this.sortable.typeSort)
    this.refresh()
  }

  changeSortableByName(sort: string, sortType: string){
    this.elementRefSortable.forEach((item) => {
      if(item.childValue.sort != sort){
        item.childValue.typeSort = ''
      }
      else{
        item.childValue.typeSort = sortType
      }
    })
    this.ref.detectChanges()
  }
  public theadTable: THeadTable[] = [
    {name: 'STT'},
    {name: 'Timesheet Account', sortName: 'fullName', defaultSort: ''},
    {name: 'Email', sortName: 'emailAddress', defaultSort: ''},
    {name: 'Computer Name', sortName: 'computerName', defaultSort: ''},
    {name: 'Date At', sortName: 'dateAt', defaultSort: ''},
    {name: 'Working Hours', sortName: 'workingMinute', defaultSort: 'DESC'},
  ] 
  styleThead(item: any){
    return {
      width: item.width,
      height: item.height
    }
  }
}

export class THeadTable{
  name: string;
  width?: string = 'auto';
  height?: string = 'auto';
  backgroud_color?: string;
  sortName?: string;;
  defaultSort?: string;
}
