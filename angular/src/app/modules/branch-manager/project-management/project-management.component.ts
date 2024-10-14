import { Component, Injector, Input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { FilterDto, PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { ProjectDto } from '../Dto/branch-manage-dto';
import { ManageUserForBranchService } from '@app/service/api/manage-user-for-branch.service';
import { Chart } from 'chart.js'
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { finalize } from 'rxjs/operators';
import { DateInfo } from '../date-filter/date-filter.component';
import { MatDialog } from '@angular/material/dialog';
import {
    ProjectManagementMemberDetailComponent
} from '@app/modules/branch-manager/modal/project-management-modal/project-management-member-detail.component';
import { APP_CONFIG } from '@app/constant/api-config.constant';
import { SortOrder, ProjectMemberType, UserTypeCount } from '../modal/project-management-modal/enum/sort-member-effort.enum';

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
  startDate: string;
  endDate: string;
  public UserTypeSearch: FormControl = new FormControl("")
  filterUserType = APP_CONFIG.EnumValueOfUserType
  userTypeId: ProjectMemberType = ProjectMemberType.All;
  sortOrder: SortOrder = SortOrder.Descending;
  userTypeMap = [ UserTypeCount.Expose, UserTypeCount.Shadow,UserTypeCount.Deactive,UserTypeCount.All];
  public SortOrderType = SortOrder;
  public filterItems: FilterDto[] = [];
  public projects: ProjectDto[];
  private projectNames: string[] = [];
  private deactiveCount: number[] = [];
  private exposeCount: number[] = [];
  private shadowCount: number[] = [];
  private filterBranchId: any;
  private chart: Chart;
  constructor(
    injector: Injector,
    private dialog: MatDialog,
    private manageUserForBranchService: ManageUserForBranchService,
  ) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    })
    this.chart = null
  }

  ngOnInit() {
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
            onClick: (event, elements) => {
                if (elements && elements.length > 0) {
                    const indexData = elements[0]._index;
                      const dialogRef = this.dialog.open(ProjectManagementMemberDetailComponent, {
                          data: {
                              projectItem: {
                                  branchId : this.branchId != 0 ? this.branchId : '',
                                  projectId: this.projects[indexData].projectId,
                                  startDate: this.startDate,
                                  endDate: this.endDate,
                                  projectName: this.projects[indexData].projectName
                              },
                          },
                          height: 'auto', width: 'auto'
                      });
                      dialogRef.afterClosed().subscribe(result => {
                          if (result) {
                              abp.notify.success('Updated successfully');
                              this.refresh();
                          }
                      });
                }
            },
            legend: {
                onHover: (e) => {
                   e.target.style.cursor = 'pointer';
                }
            },
            hover: {
                onHover: function(e) {
                   const point = this.getElementAtEvent(e);
                   if (point.length) {
                      e.target.style.cursor = 'pointer';
                   } else {
                      e.target.style.cursor = 'default';
                   }
                }
            },
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
            },
            // {
            //   label: 'PM',
            //   data: this.pmCount,
            //   backgroundColor: "rgb(244, 67, 54)",
            //   stack: 'total'
            // },
            {
              label: 'Deactive',
              data: this.deactiveCount,
              backgroundColor: "rgb(254,176,25)",
              stack: 'total'
            }]
          }
        }
      );
    }else{
      const newData = [this.shadowCount, this.exposeCount, this.deactiveCount];
      this.chart.data.labels = this.projectNames;
      this.chart.data.datasets.forEach((dataset, index) => {
        dataset.data = newData[index];
      });
      this.chart.update();
    }
  }

  private loadProjectCountData(){
    this.projects.forEach(project => {
      this.projectNames.push(project.projectName);
      this.deactiveCount.push(project.deactiveCount);
      this.exposeCount.push(project.memberCount);
      this.shadowCount.push(project.shadowCount );
  })
}
  private sortProject() {
    this.projects.sort((a, b) => {
      if (this.userTypeId === 0) {
          const aTotal = a.memberCount;
          const bTotal = b.memberCount;
          return this.sortOrder === SortOrder.Ascending ? aTotal - bTotal : bTotal - aTotal;
      }
      else {
          const field = this.userTypeMap[this.userTypeId];
          console.log(field);
          const aCount = a[field];
          const bCount = b[field];
          return this.sortOrder === SortOrder.Ascending ? aCount - bCount : bCount - aCount;
      }
    });
    // this.resetDataChart()
    // this.loadProjectCountData()
    this.showChart()
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
        this.sortProject()
        this.showPaging(rs.result, pageNumber);
        this.loadProjectCountData()
      }
      this.showChart()
    })
  }


  resetDataChart(){
    this.projectNames = [];
    this.deactiveCount = [];
    this.exposeCount = [];
    this.shadowCount = [];
    // this.pmCount = [];
  }

  searchOrFilter(): void{
    this.refresh();
    // this.sortProject()
  }
  updateSortOrder() {
    this.refresh();
    // this.sortProject()
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

  onDateSelected(dateInfo: DateInfo) {
    this.startDate=dateInfo.startDate;
    this.endDate=dateInfo.endDate;
    this.refresh();
  }
}
