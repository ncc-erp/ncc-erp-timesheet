import { Component, Inject, Injector, OnInit, Optional } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { GetProjectDto } from '@app/service/api/model/project-Dto';
import { ReportTimeSheetDto } from '@app/service/api/model/report-timesheet-Dto';
import { ProjectDetailService } from '@app/service/api/project-detail.service';
import { ExportService } from '@app/service/export.service';
import { AppComponentBase } from '@shared/app-component-base';
import { convertHourtoMinute, convertMinuteToHour } from '@shared/common-time';
import * as _ from 'lodash';
import * as moment from 'moment';
import { PopupComponent } from './popup/popup.component';

@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.css']
})
export class ProjectDetailComponent extends AppComponentBase implements OnInit {
  EXPORT_EXCEL_PROJECT = PERMISSIONS_CONSTANT.ExportExcelProject;

  detailTeam: any;
  detailTeamMock: any;
  totalTimeTeam: any;
  mapTasks = [];
  totalHour: any = {};
  projectId: any;
  datapro: any;
  totalBillTaskHour: string;
  totalHourNonBillaleTask: any
  activeView: number = 0;
  distanceFromAndToDate = '';
  viewChange = new FormControl(this.APP_CONSTANT.TypeViewHomePage.Week);
  fromDate: any;
  toDate: any;
  typeDate: any;
  percentTime: number;
  totalWorkingTime: any;
  totalBillTime: string;
  totalBillTaskBillHour: string;
  percentTask: number;
  percentTeam: number;
  exportProjects = [] as ReportTimeSheetDto[];
  project: GetProjectDto;
  constructor(
    injector: Injector,
    private fb: FormBuilder,
    private projectmanager: ProjectDetailService,
    private exportService: ExportService,
    private _dialogRef: MatDialogRef<ProjectDetailComponent>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any,
    private _dialog: MatDialog,
  ) {
    super(injector)
  }

  ngOnInit() {
    this.projectId = this.data.id;
    this.project = this.data;
    this.changeView(true);
  }
 
  buildTaskData(data: Array<TaskDto>) {
    return _(data)
      .groupBy(x => x.billable)
      .map((value, key) => (
        {
          name: key == 'true' ? 'Billable Tasks' : 'Non-billable Tasks',
          billable: key == 'true',
          tasks: value,
        } as MapTasksDto))

      .value()
      .sort(function (a, b) {
        if (a.billable == false && b.billable == true) {
          return 1;
        }
        return -1;
      });

  }

  //getdata
  getData(projectId, fromDate, toDate) {
    this.projectmanager.GetTimeSheetStatisticTasks(projectId, fromDate, toDate).subscribe(res => {
      let tasks = res.result;

      tasks.forEach(task => {
        task.percent = task.totalWorkingTime > 0 ? Math.round(task.billableWorkingTime * 100 / task.totalWorkingTime) : 0;
      });
      this.mapTasks = this.buildTaskData(tasks);
      let sumBillTask = 0;
      this.mapTasks.forEach(element => {
        element.tasks.forEach(i => {
          if (i.billable == true) {
            sumBillTask += i.billableWorkingTime;
          }
        });
      })
      this.totalBillTaskBillHour = convertMinuteToHour(sumBillTask);

      let sum = 0;
      let sumNon = 0;
      this.mapTasks.forEach(element => {
        element.tasks.forEach(i => {

          if (i.billable == true) {
            sum += i.totalWorkingTime;
          }
          if (i.billable == false) {
            sumNon += i.totalWorkingTime;
          }

        });
      })
      this.totalBillTaskHour = convertMinuteToHour(sum);
      this.totalHourNonBillaleTask = convertMinuteToHour(sumNon);
      this.percentTask = convertHourtoMinute(this.totalBillTaskBillHour) > 0 ? Math.round(convertHourtoMinute(this.totalBillTaskBillHour) * 100 / convertHourtoMinute(this.totalBillTaskHour)) : 0;
      this.mapTasks.forEach(element => {
        element.tasks.forEach(i => {
          i.totalWorkingTime = convertMinuteToHour(i.totalWorkingTime);
        });
      })
    })

  }

  getTeam(projectId, fromDate, toDate) {
    this.projectmanager.getProjectDetailTeam(projectId, fromDate, toDate).subscribe(res => {
      let teams = res.result;

      teams.forEach(s => {
        s.percent = s.totalWorkingTime > 0 ? Math.round(s.billableWorkingTime * 100 / s.totalWorkingTime) : 0;
      })


      this.detailTeam = (teams);
      this.detailTeamMock = res.result
      let sum = 0;
      this.detailTeamMock.forEach(element => {
        sum += element.totalWorkingTime;
      });
      this.totalTimeTeam = convertMinuteToHour(sum);
      this.totalTimeTeam = convertMinuteToHour(sum);
      this.detailTeam.forEach(element => {
        element.totalWorkingTime = element.totalWorkingTime;
      })
      let userTotalHour = 0;
      let TotalBillableHour = 0;
      this.detailTeam.forEach(s => {
        userTotalHour += (s.totalWorkingTime);
        TotalBillableHour += s.billableWorkingTime;
      })
      this.totalBillTime = convertMinuteToHour(TotalBillableHour);
      this.totalWorkingTime = convertMinuteToHour(userTotalHour);
      this.percentTeam = convertHourtoMinute(this.totalBillTime) > 0 ? Math.round(convertHourtoMinute(this.totalBillTime) * 100 / convertHourtoMinute(this.totalWorkingTime)) : 0;



    })
  }

  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }

  changeView(reset?: boolean, fDate?: any, tDate?: any) {
    if (reset) {
      this.activeView = 0;
    }
    let fromDate, toDate;
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Week) {
      fromDate = moment().startOf('isoWeek').add(this.activeView, 'w');
      toDate = moment(fromDate).endOf('isoWeek');
      this.typeDate = 'Week';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Month) {
      fromDate = moment().startOf('M').add(this.activeView, 'M');
      toDate = moment(fromDate).endOf('M');
      this.typeDate = 'Month';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Quater) {
      fromDate = moment().startOf('Q').add(this.activeView, 'Q');
      toDate = moment(fromDate).endOf('Q');
      this.typeDate = 'Quater';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Year) {
      fromDate = moment().startOf('y').add(this.activeView, 'y');
      toDate = moment(fromDate).endOf('y');
      this.typeDate = 'Years';
    }
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.AllTime) {
      fromDate = '';
      toDate = '';
      this.distanceFromAndToDate = 'All Time';
    }
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      fromDate = '';
      toDate = '';
      if (!reset && fDate && tDate) {
        if (fDate && tDate) {
          fromDate = fDate.format('DD MMM YYYY');
          toDate = tDate.format('DD MMM YYYY');
        }
        this.setFromAndToDate(fromDate, toDate);
        this.getData(this.projectId, fromDate, toDate);
        this.getTeam(this.projectId, fromDate, toDate)


        this.distanceFromAndToDate = fromDate + '  -  ' + toDate
      } else {
        this.distanceFromAndToDate = 'Custom Time';
      }
    }

    if (fromDate != '' && toDate != '') {

      let fDate = '', tDate = '';
      let list = [];
      list[0] = { value: fromDate.isSame(toDate, 'year'), type: 'YYYY' };
      list[1] = { value: fromDate.isSame(toDate, 'month'), type: 'MMM' };
      list[2] = { value: fromDate.isSame(toDate, 'day'), type: 'D' };
      list.map(value => {
        if (value.value) {
          tDate = toDate.format(value.type) + ' ' + tDate;
        } else {
          fDate = fromDate.format(value.type) + ' ' + fDate;
          tDate = toDate.format(value.type) + ' ' + tDate;
        }
      })
      this.distanceFromAndToDate = this.typeDate + ': ' + fDate + ' - ' + tDate;

    }
    if (this.viewChange.value != this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      fromDate = fromDate == '' ? '' : fromDate.format('YYYY-MM-DD');
      toDate = toDate == '' ? '' : toDate.format('YYYY-MM-DD');
      this.getData(this.projectId, fromDate, toDate);
      this.getTeam(this.projectId, fromDate, toDate)
      this.setFromAndToDate(fromDate, toDate);
    }
  }

  nextOrPre(title: any) {
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      return
    }
    if (title == 'pre') {
      this.activeView--;
    }
    if (title == 'next') {
      this.activeView++;

    }
    this.changeView();
  }

  showPopup(): void {
    let popup = this._dialog.open(PopupComponent);
    popup.afterClosed().subscribe(result => {
      if (result != undefined) {
        if (result.result) {
          this.changeView(false, result.data.fromDateCustomTime, result.data.toDateCustomTime);
        }
      }
    });
  }

  exportProject(): void {
    let fileName = 'Invoice_' + this.project.customerName + '_' + this.project.code + '_' + this.project.name + '_' + this.fromDate + '_' + this.toDate;
    fileName.replace(/ /g, '-');
    this.projectmanager.getExportBillableTimesheets(this.projectId, this.fromDate, this.toDate)
      .subscribe(res => {
        this.exportProjects = res.result;
        this.exportService.exportTimeSheetProject(this.exportProjects, fileName);
      });
  }

}


export class MapTasksDto {
  name: string;
  tasks: TaskDto[];
  billable: boolean;
}
export class TaskDto {
  billable: boolean;
  billableWorkingTime: number;
  taskId: number;
  taskName: string;
  totalWorkingTime: string;
  percent: number;
}
export class MapteamDto {
  name: string;
  team: TeamDto[];
}
export class TeamDto {
  billableWorkingTime: number;
  projectUserType: number;
  totalWorkingTime: number;
  userID: number;
  userName: number;
  percent: number;
}
