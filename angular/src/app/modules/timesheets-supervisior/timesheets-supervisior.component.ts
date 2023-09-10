import { ChangeDetectorRef, Component, Injector, OnInit } from '@angular/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { DomSanitizer } from '@angular/platform-browser';
import { TimeSheetDto } from '@app/service/api/model/timesheet-Dto';
import { TimesheetsSupervisiorService } from '@app/service/api/timesheets-supervisior.service';
import { AppComponentBase } from '@shared/app-component-base';
import * as _ from 'lodash';

export const MY_FORMATS = {
  parse: {
    dateInput: ['LL', 'YYYY-MM-DD'],
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },

};

@Component({
  selector: 'app-timesheets-supervisior',
  templateUrl: './timesheets-supervisior.component.html',
  styleUrls: ['./timesheets-supervisior.component.css'],
  providers: [
    { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})

export class TimesheetsSupervisiorComponent extends AppComponentBase implements OnInit {

  filterStatus: number = this.APP_CONSTANT.TimesheetStatus.Pending; // ALL
  workingType: number = this.APP_CONSTANT.EnumTypeOfWork.All; // ALL
  viewBy: number = this.APP_CONSTANT.TimesheetViewBy.Project; // Project
  timesheetsGroup: any[] = []; // Store the final list of timesheet to render
  isLock: boolean = true;
  fromDate: any;
  toDate: any;
  typeDate: any;
  isLoading: boolean;
  rawData: TimeSheetDto[] = [];
  filteredTimesheets: TimeSheetDto[] = [];

  Timesheet_TypeOfWorks = [
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.All,
      name: 'All',
      count: 0
    },
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours,
      name: 'Normal working ',
      count: 0
    },
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.Overtime,
      name: 'OverTime',
      count: 0
    },
  ]

  Timesheet_Statuses = [
    {
      value: this.APP_CONSTANT.TimesheetStatus.All,
      name: 'All',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Draft,
      name: 'Draft',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Pending,
      name: 'Pending',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Approve,
      name: 'Approved',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Reject,
      name: 'Rejected',
      count: 0
    }
  ]

  constructor(
    injector: Injector,
    private domSanitizer: DomSanitizer,
    private timesheetSupervisiorService: TimesheetsSupervisiorService,
    private changeDetector: ChangeDetectorRef,
  ) {
    super(injector);
  }

  ngOnInit() { }

  ngAfterViewChecked() {
    this.changeDetector.detectChanges();
  }

  getData() {
    this.isLoading = true;
    this.timesheetSupervisiorService.getAll(this.fromDate, this.toDate, this.filterStatus).subscribe(obj => {
      // After the supervisior choose another date, status or view.
      this.rawData = obj.result;
      // this.convertData(obj.result);
      this.onWorkingTypeChange();
      this.getQuantityTimesheetSupervisorStatus();
      this.isLoading = false;
    });
  }
  getQuantityTimesheetSupervisorStatus(){
    this.timesheetSupervisiorService.GetQuantityTimesheetSupervisorStatus(this.fromDate, this.toDate).subscribe((obj:any)=>{
      let statusAll = this.Timesheet_Statuses.find(s => s.value == this.APP_CONSTANT.TimesheetStatus.All);
        statusAll.count = obj.result.reduce((previousValue, currentValue) => previousValue + currentValue.quantity,0);
        let statusDraft = this.Timesheet_Statuses.find(s => s.value == this.APP_CONSTANT.TimesheetStatus.Draft);
        if(obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Draft).length >0){
          statusDraft.count  = obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Draft)[0].quantity;
        }
        let statusPending = this.Timesheet_Statuses.find(s => s.value == this.APP_CONSTANT.TimesheetStatus.Pending);
        if(obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Pending).length > 0){
          statusPending.count = obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Pending)[0].quantity;
        }
        let statusApprove = this.Timesheet_Statuses.find(s => s.value == this.APP_CONSTANT.TimesheetStatus.Approve);
        if(obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Approve).length >0){
          statusApprove.count = obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Approve)[0].quantity;
        }
        let statusReject = this.Timesheet_Statuses.find(s => s.value == this.APP_CONSTANT.TimesheetStatus.Reject);
        if(obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Reject)[0].quantity){
          statusReject.count = obj.result.filter(s => s.status == this.APP_CONSTANT.TimesheetStatus.Reject)[0].quantity;
        }
    })
  }

  onWorkingTypeChange() {
    this.filteredTimesheets = this.rawData.filter(s => this.workingType === this.APP_CONSTANT.EnumTypeOfWork.All
      || s.typeOfWork === this.workingType);

    this.convertData(this.filteredTimesheets);
  }

  onGroupByChange() {
    this.convertData(this.filteredTimesheets);
  }

  handleDateSelectorChange(date) {
    const { fromDate, toDate } = date;
    this.setFromAndToDate(fromDate, toDate);
    this.getData();
  }

  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }

  convertData(timesheets: TimeSheetDto[]) {
    if (this.viewBy == this.APP_CONSTANT.TimesheetViewBy.People) {
      this.timesheetsGroup = this.buildDataByUser(timesheets);
    }
    else if (this.viewBy == this.APP_CONSTANT.TimesheetViewBy.Project) {
      this.timesheetsGroup = this.buildDataByProject(timesheets);
    }
  }

  buildDataByDate(data: Array<TimeSheetDto>): TimesheetGroupByDayDto[] {
    return _.chain(data).groupBy('dateAt').map((value, key) => ({
      dateAt: key,
      tasks: value,
      totalWorkingTime: value.map(s => s.workingTime).reduce((a, b) => a + b, 0),
    })).orderBy(x => x.dateAt, 'desc').value();
  }

  buildDataByUser(data: Array<TimeSheetDto>) {
    return _.chain(data).groupBy('userId').map((value, key) => ({
      userId: key,
      userName: value[0].user,
      totalUserWorkingTime: this.buildDataByDate(value).map(s => s.totalWorkingTime).reduce((a, b) => a + b, 0),
      dayTasks: this.buildDataByDate(value)
    })).value();
  }

  buildDataByProject(data: Array<TimeSheetDto>) {
    return _.chain(data).groupBy('projectId').map((value, key) => ({
      projectId: key,
      listPM: value[0].listPM,
      projectName: value[0].projectName,
      totalProjectWorkingTime: this.buildDataByUser(value).map(s => s.totalUserWorkingTime).reduce((a, b) => a + b, 0),
      userTasks: this.buildDataByUser(value)
    })).value();
  }

  convertNumberOfStringStatus(status: number, typeOfWork: number) {
    let html = (typeOfWork != 1) ? '' : '<span class="label label-danger" style="padding: 3px 5px; margin-left: 2npx">OT</span>';
    switch (status) {
      case 1: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-info'>Pending</span>" + html;
        break;
      }
      case 2: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-success'>Approved</span>" + html;
        break;
      }
      case 3: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-default'>Rejected</span>" + html;
        break;
      }
    }
    return this.domSanitizer.bypassSecurityTrustHtml(html);
  }
}

export class TimesheetGroupByDayDto {
  dateAt: string;
  totalWorkingTime: number;
  tasks: TimeSheetDto[]
}

export class TimesheetGroupByUserDto {
  userId: number;
  userName: string;
  dayTasks: TimesheetGroupByDayDto[]
}

export class TimesheetGroupByProject {
  projectId: number;
  projectName: string;
  userTasks: TimesheetGroupByUserDto[];
}
