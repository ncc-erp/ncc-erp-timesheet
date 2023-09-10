import { Component, Injector, OnInit, AfterViewInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { TimesheetService } from '@app/service/api/timesheet.service';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { convertMinuteToHour } from '@shared/common-time';
import * as moment from 'moment';
import { MatDialog } from '@angular/material';
import { PopupsComponent } from './popups/popups.component';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { Sort } from '@angular/material/sort';
import { getDay } from 'ngx-bootstrap/chronos/utils/date-getters';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  animations: [appModuleAnimation()],
})
export class HomeComponent extends AppComponentBase implements OnInit, AfterViewInit {
  dataTask: any = [];
  dataTeam: any = [];
  dataClient: any = [];
  dataProject: any = [];
  hoursTracked: any = '';
  dowEx: any;
  percentBill: any;
  billable;
  nonBillable;
  normalWorkingHours;
  overtimeBillable;
  overtimeNonBillable;
  timesheetReportData;
  percentBillable;
  percentWorkingHours;
  percentOverTime;
  percentTime: any;
  totalHour: any = {};
  fromDate: any;
  toDate: any;
  typeDate: any;
  sortedData: any;
  sortedDataProject: any;
  a: any;

  constructor(
    injector: Injector,
    private fb: FormBuilder,
    private timesheetsevice: MyTimesheetService,
    private projectmanagersevice: ProjectManagerService,
    private _dialog: MatDialog,

  ) {

    super(injector);
    this.sortedData = this.dataClient.slice();
  }

  ngOnInit() {
  }

  //sắp xếp dataClient
  sortData(sort: Sort) {
    const data = this.dataClient.slice();
    // console.log('data::', data)
    if (!sort.active || sort.direction === '') {
      this.sortedData = data;
      // console.log('data', data)
      return;
    }
    this.sortedData = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'customerName': return compare(a.customerName, b.customerName, isAsc);
        case 'totalWorkingTime': return compare(a.totalWorkingTime, b.totalWorkingTime, isAsc);
        case 'billableWorkingTime': return compare(a.billableWorkingTime, b.billableWorkingTime, isAsc);
        default: return 0;
      }
    });
  }
  //sắp xếp dataProject
  sortDataProject(sort: Sort) {
    const data = this.dataProject.slice();
    // console.log('data::', data)
    if (!sort.active || sort.direction === '') {
      this.sortedDataProject = data;
      // console.log('data', data)
      return;
    }
    this.sortedDataProject = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'customerName': return compare(a.customerName, b.customerName, isAsc);
        case 'totalWorkingTime': return compare(a.totalWorkingTime, b.totalWorkingTime, isAsc);
        case 'billableWorkingTime': return compare(a.billableWorkingTime, b.billableWorkingTime, isAsc);
        default: return 0;
      }
    });
  }

  ngAfterViewInit(): void {
    let fDate = moment().startOf('isoWeek').add(this.APP_CONSTANT.TypeViewHomePage.Week, 'w').format('MM-DD-YYYY');
    let tDate = moment().endOf('isoWeek').add(this.APP_CONSTANT.TypeViewHomePage.Week, 'w').format('MM-DD-YYYY');
    this.getTimesheetReport(fDate, tDate);
  }
  //lấy dữ liệu từ db qua thời gian bắt đầu và kết thúc
  getData(formDate, toDate) {
    // console.log('fromdate', this.fromDate)
    // console.log('todate', this.toDate)
    if (this.permission.isGranted('Home')) {
      this.timesheetsevice.getTaskList(formDate, toDate).subscribe(res => {
        this.dataTask = res.result;
        this.handleAfterGetData(this.dataTask, 'Task');
        // console.log('aaa',this.dataTask)
      })
      this.timesheetsevice.getTeamList(formDate, toDate).subscribe(res => {
        this.dataTeam = res.result;
        this.handleAfterGetData(this.dataTeam, 'Team');
      })
      this.timesheetsevice.getCustomerList(formDate, toDate).subscribe(res => {
        this.dataClient = res.result;
        this.handleAfterGetData(this.dataClient, 'Client');
        this.sortedData = this.dataClient
        // console.log('data client', this.dataClient)
      })
      this.timesheetsevice.getProjectList(formDate, toDate).subscribe(res => {
        this.dataProject = res.result;
        this.handleAfterGetData(this.dataProject, 'Project');
        this.sortedDataProject = this.dataProject
      })
    }
  }

  handleAfterGetData(list, name) {
    this.totalHour[`totalHour${name}`] = list.reduce((total, currentValue, currentIndex, arr) => {
      return total + currentValue.totalWorkingTime;
    }, 0)
    this.totalHour[`totalBillableHour${name}`] = list.reduce((total, currentValue, currentIndex, arr) => {
      return total + currentValue.billableWorkingTime;
    }, 0)
    this.totalHour[`totalHour${name}`] = convertMinuteToHour(this.totalHour[`totalHour${name}`]);
    this.totalHour[`totalBillableHour${name}`] = convertMinuteToHour(this.totalHour[`totalBillableHour${name}`]);
    list.map(value => {
      value.percent = value.totalWorkingTime > 0 ? (value.billableWorkingTime * 100 / value.totalWorkingTime).toFixed(0) : 0;
      // value.percent = (Math.floor((value.billableWorkingTime / value.totalWorkingTime) * 10000)) % 100 != 0 ? ((value.billableWorkingTime / value.totalWorkingTime) * 100).toFixed(2) : (value.billableWorkingTime / value.totalWorkingTime) * 100;
      value.totalWorkingTime = convertMinuteToHour(value.totalWorkingTime);
      value.billableWorkingTime = convertMinuteToHour(value.billableWorkingTime);
    })
  }

  handleDateSelectorChange(date) {
    const { fromDate, toDate } = date;
    this.getData(fromDate, toDate);
    this.getTimesheetReport(fromDate, toDate);
    this.setFromAndToDate(fromDate, toDate);
  }

  dowExcel(projectId) {
    let fromDate = this.fromDate;
    fromDate = moment(new Date(fromDate)).format("MM-DD-YYYY hh:mm:ss").toString();
    // console.log(fromDate);
    let toDate = this.toDate;
    toDate = moment(new Date(toDate)).format("MM-DD-YYYY hh:mm:ss").toString();
    this.projectmanagersevice.ExportExcel(fromDate, toDate, projectId).subscribe(res => {
      this.dowEx = res.result;
    })
  }
  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }

  setDataTimesheetReport(res) {
    this.timesheetReportData = res.result;
    this.hoursTracked = convertMinuteToHour(res.result.hoursTracked);
    this.billable = convertMinuteToHour(res.result.billable);
    this.nonBillable = convertMinuteToHour(res.result.nonBillable);
    this.normalWorkingHours = convertMinuteToHour(res.result.normalWorkingHours);
    this.overtimeBillable = convertMinuteToHour(res.result.overtimeBillable);
    this.overtimeNonBillable = convertMinuteToHour(res.result.overtimeNonBillable);
    this.percentBillable = Number(res.result.billable) / Number((res.result.billable + res.result.nonBillable)) * 100;
    this.percentOverTime = Number(res.result.overtimeBillable) / Number((res.result.normalWorkingHours + res.result.overtimeBillable + res.result.overtimeNonBillable)) * 100;
    this.percentWorkingHours = Number(res.result.normalWorkingHours) / Number((res.result.normalWorkingHours + res.result.overtimeBillable + res.result.overtimeNonBillable)) * 100;
    this.percentBill = Number(res.result.hoursTracked) / Number((this.totalHour.totalBillableHourProject)) * 100
    this.percentTime = (Number(res.result.billable) / Number(res.result.hoursTracked) * 100).toFixed(0);
    // if(this.percentTime == undefined||this.percentTime == NaN){
    //     this.percentTime = 0;
    // }
  }

  setChartArt() {
    var context = this;
    $(function () {
      // Widgets count
      $('.count-to').countTo();
      // Sales count to
      $('.sales-count-to').countTo({
        formatter: function (value, options) {
          return '$' + value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, ' ').replace('.', ',');
        }
      });
      initDonutChart();
    });

    function initDonutChart() {
      document.getElementById('donut_chart').innerHTML = '';
      document.getElementById('donut_chart1').innerHTML = '';
      let data_donut_chart = [];
      let data_donut_chart1 = [];
      if (!isNaN(context.percentBillable)) {
        data_donut_chart.push(context.percentBillable);
      }
      if (!isNaN(context.percentWorkingHours)) {
        data_donut_chart1.push(context.percentWorkingHours);
      }
      ((window as any).Morris).Donut({

        element: 'donut_chart',
        data: data_donut_chart.length ? [{
          label: 'Billable',
          value: Math.floor(context.percentBillable),
        },
        {
          label: 'No-Bill',
          value: 100 - Math.floor(context.percentBillable)
        }
        ] : [{ label: 'No Data', value: 100 }],

        colors: ['#00bcd4', '#b9f0f7',],
        formatter: function (y) {
          return y + '%';
        }
      });
      ((window as any).Morris).Donut({
        element: 'donut_chart1',
        data: data_donut_chart1.length ? [{
          label: 'Normal',
          value: Math.floor(context.percentWorkingHours)
        },
        {
          label: 'OT Bill',
          value: Math.floor(context.percentOverTime)
        },
        {
          label: 'OT No-Bill',
          value: 100 - Math.floor(context.percentWorkingHours) - Math.floor(context.percentOverTime)
        }
        ] : [{ label: 'No Data', value: 100 }],

        colors: ['#00bcd4', '#b9f0f7', 'rgb(100, 30, 200)',],
        formatter: function (y) {
          return y + '%';
        }
      });
    }
    // console.log(context);
  }

  private getTimesheetReport(fromDate, toDate) {
    if (this.permission.isGranted('Home')) {
      this.timesheetsevice.getTimesheetReport(fromDate, toDate).subscribe(res => {
        this.setDataTimesheetReport(res);
        this.setChartArt();
      })
    }
  }
}

function compare(a: number | string, b: number | string, isAsc: boolean) {
  return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}
