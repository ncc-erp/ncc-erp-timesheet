import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { FilterDto, PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { ProjectDto } from '../Dto/branch-manage-dto';
import { ManageUserForBranchService } from '@app/service/api/manage-user-for-branch.service';
import { Chart } from 'chart.js'
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { finalize } from 'rxjs/operators';
import * as moment from 'moment';
import { MatDatepicker } from '@angular/material';

@Component({
  selector: 'app-project-management',
  templateUrl: './project-management.component.html',
  styleUrls: ['./project-management.component.css']
})
export class ProjectManagementComponent extends PagedListingComponentBase<any> implements OnInit {
  ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs = PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];
  public branchSearch: FormControl = new FormControl("")
  branchId;

  typeOfView = this.APP_CONSTANT.TypeViewBranchManager.Month;
  startView = "year";
  activeDay: any;
  startDate: string;
  endDate: string;
  displayDay: any;
  TimeType = Object.keys(this.APP_CONSTANT.TypeViewBranchManager);

  public filterItems: FilterDto[] = [];
  public projects: ProjectDto[];
  private projectNames: string[] = [];
  private memberCount: number[] = [];
  private exposeCount: number[] = [];
  private shadowCount: number[] = [];
  private filterBranchId: any;
  private chart: Chart;
  constructor(
    injector: Injector,
    private manageUserForBranchService: ManageUserForBranchService,
  ) {
    super(injector);
    this.branchId = 0;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    })
    this.chart = null
  }

  ngOnInit() {
    this.startDate = moment().startOf('M').format('YYYY-MM-DD');
    this.endDate = moment().endOf('M').format('YYYY-MM-DD');
    this.activeDay = this.startDate;
    this.displayDay = this.startDate + " - " + this.endDate
    this.refresh();
  }

  filterBranch(): void{
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    }else{
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  showChart(){
    if(!this.chart){
      this.chart = new Chart( 
        document.getElementById('myChart'),{
          type: 'horizontalBar',
          tooltips: {enabled: true},
          legend: {display: false},
          responsive: true,
          options: {
            scales: {
              xAxes: [{
                barPercentage: 0.5,
              }],
            }
          },
          data: {
            labels: this.projectNames,
            datasets: [{
              label: 'Shadow',
              data: this.shadowCount,
              backgroundColor: "rgb(0,143,251)",
              stack: 'total'
            },{
              label: 'Expose',
              data: this.exposeCount,
              backgroundColor: "rgb(0,227,150)",
              stack: 'total'
            },{
              label: 'Member',
              data: this.memberCount,
              backgroundColor: "rgb(254,176,25)",
              stack: 'total'
            }]
          }
        }
      );
    }else{
      const newData = [this.shadowCount, this.exposeCount, this.memberCount];
      this.chart.data.labels = this.projectNames;
      this.chart.data.datasets.forEach((dataset, index) => {
        dataset.data = newData[index];
      });
      this.chart.update();
    }
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.removeFilterItem();
    if (this.branchId != 0) {
      this.filterBranchId = this.branchId;
    } else {
      this.filterBranchId = '';
    }
    request.filterItems = this.filterItems;
    this.manageUserForBranchService.getAllValueOfUserInProjectByUserId(request, this.filterBranchId, this.startDate, this.endDate)
    .pipe(
      finalize(() => {
        finishedCallback()
      })
    ).subscribe((rs: any) => {
      this.resetDataChart()
      this.totalItems = rs.result.totalCount;
      if (rs.result == null || rs.result.items.length == 0) {
        this.projects = []
      }else{
        this.projects = rs.result.items
        this.showPaging(rs.result, pageNumber);
        this.projects.forEach(project => {
          this.projectNames.push(project.projectName);
          this.memberCount.push(project.memberCount);
          this.exposeCount.push(project.exposeCount);
          this.shadowCount.push(project.shadowCount);
        })
      }
      this.showChart()
    })
  }

  resetDataChart(){
    this.projectNames = [];
    this.memberCount = [];
    this.exposeCount = [];
    this.shadowCount = [];
  }

  searchOrFilter(): void{
    this.refresh();
  }

  // clearSearchAndFilter(){
  //   this.resetDataChart()
  //   this.branchId = 0;
  //   this.refresh();
  // }

  removeFilterItem(): void {
    this.filterItems = [];
  }

  isShowSelectBranch(){
    return this.isGranted(
      PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs
    )
  }

  protected delete(entity: any): void {
    throw new Error('Method not implemented.');
  }

  customDate() {
    switch(this.typeOfView){
      case this.APP_CONSTANT.TypeViewBranchManager.Day:
        this.displayDay = moment(this.activeDay).format('YYYY-MM-DD');
        this.startDate = this.displayDay;
        this.endDate = this.displayDay;
        this.startView = "month";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Week:
        this.startDate = moment(this.activeDay).startOf('isoWeek').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('isoWeek').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "month";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Month:
        this.startDate = moment(this.activeDay).startOf('M').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('M').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "year";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Year:
        this.startDate = moment(this.activeDay).startOf('y').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('y').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "multi-year";
        break;
      default:
        this.startDate = "";
        this.endDate = "";
    }
    this.refresh();
  }

  FilterByTime() {
    this.activeDay=moment().format('YYYY-MM-DD');
    this.customDate();
  }
  setMonth(normalizedMonthAndYear: moment.Moment, datepicker: MatDatepicker<moment.Moment>) {
    if(this.typeOfView == this.APP_CONSTANT.TypeViewBranchManager.Month){
      this.activeDay=moment(normalizedMonthAndYear).format('YYYY-MM-DD');
      datepicker.close();
      this.customDate();
    }
  }
  setYear(normalizedMonthAndYear: moment.Moment, datepicker: MatDatepicker<moment.Moment>) {
    if(this.typeOfView == this.APP_CONSTANT.TypeViewBranchManager.Year){
      this.activeDay=moment(normalizedMonthAndYear).format('YYYY-MM-DD');
      datepicker.close();
      this.customDate();
    }
  }
}