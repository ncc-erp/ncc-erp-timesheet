import {
  ChangeDetectorRef,
  Component,
  Injector,
  Input,
  OnInit,
} from "@angular/core";
import { FormControl } from "@angular/forms";
import {
  FilterDto,
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { ProjectDto } from "../Dto/branch-manage-dto";
import { ManageUserForBranchService } from "@app/service/api/manage-user-for-branch.service";
import { Chart } from "chart.js";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { finalize } from "rxjs/operators";
import { DateInfo } from "../date-filter/date-filter.component";
import { MatDialog } from "@angular/material/dialog";
import { ProjectManagementMemberDetailComponent } from "@app/modules/branch-manager/modal/project-management-modal/project-management-member-detail.component";
import { APP_CONFIG } from "@app/constant/api-config.constant";
import {
  SortOrder,
  ProjectMemberType,
  UserTypeCount,
} from "../modal/project-management-modal/enum/sort-member-effort.enum";
import {
  CdkDragDrop,
  CdkDragMove,
  moveItemInArray,
} from "@angular/cdk/drag-drop";
export interface ProjectChips {
  name: string;
  sortType: SortOrder;
  priority: number;
}
@Component({
  selector: "app-project-management",
  templateUrl: "./project-management.component.html",
  styleUrls: ["./project-management.component.css"],
})
export class ProjectManagementComponent
  extends PagedListingComponentBase<any>
  implements OnInit
{
  ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs =
    PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs;
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];
  public branchSearch: FormControl = new FormControl("");
  branchId;
  isChartView: boolean = true;
  startDate: string;
  endDate: string;
  viewMode: string = 'chart';
  public UserTypeSearch: FormControl = new FormControl("");
  filterUserType = APP_CONFIG.EnumValueOfUserType;
  userTypeId: ProjectMemberType = ProjectMemberType.All;
  sortOrder: SortOrder = SortOrder.Descending;
  userTypeMap = [
    UserTypeCount.Expose,
    UserTypeCount.Shadow,
    UserTypeCount.Deactive,
    UserTypeCount.All,
  ];
  public headerSortMap: Map<string, SortOrder> = new Map<string, SortOrder>();
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
    private cdr: ChangeDetectorRef
  ) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
    this.chart = null;
  }

  ngOnInit() {}

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter((data) =>
        data.displayName
          .toLowerCase()
          .includes(this.branchSearch.value.toLowerCase().trim())
      );
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  showChart() {
    if (!this.chart) {
      // setTimeout(() => {

      // }, 50);
      this.chart = new Chart(document.getElementById("myChart"), {
        type: "horizontalBar",
        tooltips: { enabled: true },
        legend: { display: false },
        responsive: true,
        options: {
          onClick: (event, elements) => {
            if (elements && elements.length > 0) {
              const indexData = elements[0]._index;
              const dialogRef = this.dialog.open(
                ProjectManagementMemberDetailComponent,
                {
                  data: {
                    projectItem: {
                      branchId: this.branchId != 0 ? this.branchId : "",
                      projectId: this.projects[indexData].projectId,
                      startDate: this.startDate,
                      endDate: this.endDate,
                      projectName: this.projects[indexData].projectName,
                    },
                  },
                  height: "auto",
                  width: "auto",
                }
              );
              dialogRef.afterClosed().subscribe((result) => {
                if (result) {
                  abp.notify.success("Updated successfully");
                  this.refresh();
                }
              });
            }
          },
          legend: {
            onHover: (e) => {
              e.target.style.cursor = "pointer";
            },
          },
          hover: {
            onHover: function (e) {
              const point = this.getElementAtEvent(e);
              if (point.length) {
                e.target.style.cursor = "pointer";
              } else {
                e.target.style.cursor = "default";
              }
            },
          },
          scales: {
            xAxes: [
              {
                barPercentage: 0.5,
              },
            ],
          },
        },
        data: {
          labels: this.projectNames,
          datasets: [
            {
              label: "Shadow",
              data: this.shadowCount,
              backgroundColor: "rgb(0,143,251)",
              stack: "total",
            },
            {
              label: "Expose",
              data: this.exposeCount,
              backgroundColor: "rgb(0,227,150)",
              stack: "total",
            },
            // {
            //   label: 'PM',
            //   data: this.pmCount,
            //   backgroundColor: "rgb(244, 67, 54)",
            //   stack: 'total'
            // },
            {
              label: "Deactive",
              data: this.deactiveCount,
              backgroundColor: "rgb(254,176,25)",
              stack: "total",
            },
          ],
        },
      });
    } else {
      const newData = [this.shadowCount, this.exposeCount, this.deactiveCount];
      this.chart.data.labels = this.projectNames;
      this.chart.data.datasets.forEach((dataset, index) => {
        dataset.data = newData[index];
      });
      this.chart.update();
    }
  }

  private loadProjectCountData() {
    this.projects.forEach((project) => {
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
          const aCount = a[field];
          const bCount = b[field];
          return this.sortOrder === SortOrder.Ascending ? aCount - bCount : bCount - aCount;
      }
    });
    this.resetDataChart()
    this.loadProjectCountData()
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
      this.filterBranchId = "";
    }
    request.filterItems = this.filterItems;
    this.manageUserForBranchService
      .getAllValueOfUserInProjectByUserId(
        request,
        this.filterBranchId,
        this.startDate,
        this.endDate
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((rs: any) => {
        this.resetDataChart();
        this.totalItems = rs.result.totalCount;

        if (rs.result == null || rs.result.items.length == 0) {
          this.projects = [];
        } else {
          this.projects = rs.result.items;
          this.showPaging(rs.result, pageNumber);
          // this.loadProjectCountData();
          this.sortProject();
        }
        this.showChart();
      });
  }

  resetDataChart() {
    this.projectNames = [];
    this.deactiveCount = [];
    this.exposeCount = [];
    this.shadowCount = [];
  }

  searchOrFilter(): void{
    this.refresh();
  }
  updateSortOrder() {
    this.refresh();
  }

  removeFilterItem(): void {
    this.filterItems = [];
  }

  isShowSelectBranch() {
    return this.isGranted(
      PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs
    );
  }

  protected delete(entity: any): void {
    throw new Error("Method not implemented.");
  }

  onDateSelected(dateInfo: DateInfo) {
    this.startDate = dateInfo.startDate;
    this.endDate = dateInfo.endDate;
    this.refresh();
  }

  toggleView(view: string) {
    if (view === "chart") {
      this.isChartView = true;
      setTimeout(() => {
        this.showChart();
      }, 50);
    } else {
      this.isChartView = false;
      this.chart = null;
    }
  }
  openProjectDetail(project: any) {
    const dialogRef = this.dialog.open(ProjectManagementMemberDetailComponent, {
      data: {
        projectItem: {
          branchId: this.branchId != 0 ? this.branchId : "",
          projectId: project.projectId,
          startDate: this.startDate,
          endDate: this.endDate,
          projectName: project.projectName,
        },
      },
      height: "auto",
      width: "auto",
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        abp.notify.success("Updated successfully");
        this.refresh();
      }
    });
  }

  //Chips
  selectable = true;
  removable = true;
  projectChips: ProjectChips[] = [];

  add(header: string): void {
    const sortType = this.headerSortMap.get(header);
    const existingChipIndex = this.projectChips.findIndex(
      (chip) => chip.name === header
    );
    if (existingChipIndex !== -1) {
      this.projectChips[existingChipIndex].sortType = sortType;
    } else {
      this.projectChips.push({
        name: header,
        sortType,
        priority: this.projectChips.length + 1,
      });
    }
  }

  remove(projectChip: ProjectChips): void {
    const index = this.projectChips.indexOf(projectChip);
    if (index >= 0) {
      this.projectChips.splice(index, 1);
      this.headerSortMap.set(projectChip.name, undefined);
    }
    this.updateChipPriorities();
    this.sortProjectsTable();
  }
  removeAllChips(): void {
  this.projectChips = [];
  this.headerSortMap.clear();
  this.sortProjectsTable();
}
  toggleHeaderSortType(header: string): void {
    const currentSortOrder = this.headerSortMap.get(header);
    const newSortOrder =
      currentSortOrder === SortOrder.Ascending
        ? SortOrder.Descending
        : SortOrder.Ascending;
    this.headerSortMap.set(header, newSortOrder);
    this.add(header);
    this.sortProjectsTable();
  }
  //drab and drop
  drop(event: CdkDragDrop<string[]>): void {
    moveItemInArray(this.projectChips, event.previousIndex, event.currentIndex);
    this.updateChipPriorities();
    this.sortProjectsTable();
  }
  cdkDragMoved(event: CdkDragMove) {}
  updateChipPriorities(): void {
    this.projectChips.forEach((chip, index) => {
      chip.priority = index + 1;
    });
  }
  sortProjectsTable(): void {
    this.projects.sort((a, b) => {
      for (let chip of this.projectChips) {
        const field = this.mapChipToProjectField(chip.name);
        const sortType = chip.sortType;
        const comparison = this.compareByField(a, b, field, sortType);
        if (comparison !== 0) {
          return comparison;
        }
      }

      return 0;
    });
  }

  compareByField(a: any, b: any, field: string, sortType: SortOrder): number {
    const aValue = a[field];
    const bValue = b[field];
    if (aValue < bValue) {
      return sortType === SortOrder.Ascending ? -1 : 1;
    }
    if (aValue > bValue) {
      return sortType === SortOrder.Ascending ? 1 : -1;
    }
    return 0;
  }

  mapChipToProjectField(chipName: string): string {
    switch (chipName) {
      case "Project Name":
        return "projectName";
      case "Expose":
        return "memberCount";
      case "Shadow":
        return "shadowCount";
      case "Deactive":
        return "deactiveCount";
      case "Total":
        return "totalUser";
      case "PM":
        return "pmCount";
      default:
        return "";
    }
  }
}
