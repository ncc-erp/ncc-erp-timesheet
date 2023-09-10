import { Component, Injector, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { userDTO } from '@app/modules/user/user.component';
import { BranchService } from '@app/service/api/branch.service';
import { TardinessDto, TimekeepingDto } from '@app/service/api/model/report-timesheet-Dto';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { UserService } from '@app/service/api/user.service';
import { ExportService } from '@app/service/export.service';
import { AppConsts } from '@shared/AppConsts';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { finalize } from 'rxjs/operators';
import { SelectedDateComponent } from './selected-date/selected-date.component';

@Component({
  selector: 'app-tardiness-leave-early',
  templateUrl: './tardiness-leave-early.component.html',
  styleUrls: ['./tardiness-leave-early.component.css']
})
export class TardinessLeaveEarlyComponent extends PagedListingComponentBase<TimekeepingDto> implements OnInit {
  VIEW_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewTardinessLeaveEarly;
  GET_DATA_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.GetDataFromFaceId;
  EXPORT_EXCEL = PERMISSIONS_CONSTANT.ExportExcelTardinessLeaveEarly;
  VIEW_ONLY_ME_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewOnlyMeTardinessLeaveEarly;


  listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  listYear = APP_CONSTANT.ListYear;
  month;
  year;
  branchId;
  userSearch = new FormControl();
  userControl = new FormControl();
  listUserBase = [];
  listUserFiltered: userDTO[];
  isLoadingFileUpload: boolean;
  isFileSupported: boolean;
  fileSCROM: File;
  listUser: TardinessDto[] = [];
  listSendUser: TardinessDto[] = [];
  name = "";
  userId = 0;

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter : BranchDto[];

  constructor(
    private userService: UserService,
    private activatedRoute: ActivatedRoute,
    private timekeepingService: TimekeepingService,
    private exportService: ExportService,
    private branchService: BranchService,
    injector: Injector,
    private router:Router,
    private dialog: MatDialog
  ) {
    super(injector);
    var d = new Date();
    let now = d.toLocaleDateString();
    console.log(now);
    d.setMonth(d.getMonth());
    const month = Number.parseInt(this.activatedRoute.snapshot.queryParamMap.get("month"));
    this.month = month ? month - 1 : d.getMonth();
    this.year = d.getFullYear();
    this.branchId = 0;
    this.userSearch.valueChanges.subscribe(() => {
      this.search();
    });
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    request.filterItems = [];

    // if (this.branchId !== 0) {
    //   request.filterItems.push({
    //     comparison: 0,
    //     propertyName: 'branchId',
    //     value: this.branchId,
    //   });
    // }
    this.timekeepingService.getAllPagging(request, Number(this.year), Number(this.month) + 1, Number(this.branchId), Number(this.userId))
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe(resp => {
        this.listUser = resp.result.items;
        this.showPaging(resp.result, pageNumber);
      });
  }
  protected delete(entity: TimekeepingDto): void {
    throw new Error('Method not implemented.');
  }

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

  getData() {

      this.refresh();
      if(this.permission.isGranted( this.VIEW_TARDINESS_LEAVE_EARLY)){
      this.userService.getAllNotPagging().subscribe(result => {
        this.listUserBase = result.result;
        this.listUserBase.unshift({
          id: -1,
          emailAddress: "All",
        })
        this.init();
      });

    }

  }

  exportExcel() {
    let fileName = "Report Tardiness " + "Month " + (this.month + 1) + " - Year " + this.year;
    this.exportService.exportReportTardiness(this.listUser, fileName);
  }

  getCss(value){
    value = Number.parseInt(value);
    if(value > 0) return "red";
    return "normal";
  }

  formatDate(date: string) {
    return new Date(date).toLocaleDateString("vi");
  }

  convertTime(time: string) {
    if (time.includes('.')) {
      const t = time.split('.');
      const h = ('0' + t[0]).slice(-2);
      const m = ('0' + Number.parseInt(t[1]) * 6).slice(-2);
      return h + ":" + m;
    } else {
      const h = ('0' + time).slice(-2);
      const m = '00';
      return h + ":" + m;
    }
  }

  init(): void {
    this.listUserFiltered = this.listUserBase.filter(item => item.branchId == this.branchId || !this.branchId);
  }

  search(): void {
    var temp = this.userSearch.value.trim().toLowerCase();
    this.listUserFiltered = this.listUserBase.filter(data =>
      data.emailAddress.toLowerCase().includes(temp)
    );
  }

  getDataCheckInInternal(): void {
    const dialogRef = this.dialog.open(SelectedDateComponent, {
      disableClose: true
    });

  }

  upLoadTimekeeping(file: File) {
    if (!file.name.toLowerCase().endsWith('xlsx')) {
      this.isFileSupported = false;
      return;
    }
    this.isLoadingFileUpload = true;
    this.fileSCROM = file;

    this.timekeepingService.ImportTimekeepingFromFile(file).subscribe(data => {
      this.isLoadingFileUpload = false;
      let result = data.body.result;
      if (result.failedList.length > 0) {
        let info = '<b>Success: ' + result.successList.length + '. Failed: ' + result.failedList.length + '</b><ul>';
        result.failedList.forEach(elem => {
          info += `<li>${elem}</li>`;
        });
        info += '<ul>'
        abp.message.info(info, 'Upload result', true);
        this.refresh();
      }
      else {
        abp.message.success('Success', 'Upload result');
        this.refresh();
      }
    }, () => {
      this.isLoadingFileUpload = false;
      abp.notify.error(this.l("The file is not supported!"));
    });
  }

  viewDetail(userId:number){
    this.router.navigate(["/app/main/tardiness-leave-early-detail"], {
      queryParams: {
        id: userId,
        month: this.month + 1
      }
    })
  }
  changeRouter(){
    this.router.navigate(["/app/main/tardiness-leave-early"], {
      queryParams: {
        month: this.month + 1
      }
    })
  }
}
