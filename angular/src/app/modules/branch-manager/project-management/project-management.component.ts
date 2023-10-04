import { Component, Injector, OnInit } from '@angular/core';
import { ManageUserForBranchService } from '@app/service/api/manage-user-for-branch.service';
import { FilterDto, PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { Chart } from 'chart.js'
import { finalize } from 'rxjs/operators';


@Component({
  selector: 'app-project-management',
  templateUrl: './project-management.component.html',
  styleUrls: ['./project-management.component.css']
})
export class ProjectManagementComponent extends PagedListingComponentBase<any> implements OnInit {
  public filterItems: FilterDto[] = [];
  public projects: projectDTO[];
  constructor(
    injector: Injector,
    private manageUserForBranchService:ManageUserForBranchService ,
  ) {
    super(injector);
  }

  ngOnInit() {
    new Chart( 
      document.getElementById('myChart'),{
        type: 'horizontalBar',
        tooltips: {
          enabled: true
        },
        legend: {
          display: false
        },
        responsive: true,
        options: {
        },
        data: {
          labels: ["2015", "2014", "2013", "2012", "2011"],
          datasets: [{
            label: 'Shadow',
            data: [727, 589, 537, 543, 574],
            backgroundColor: "rgb(0,143,251)",
            hoverBackgroundColor: "rgb(0,143,251)",
            stack: 'total'
          },{
            label: 'Expose',
            data: [238, 553, 746, 884, 903],
            backgroundColor: "rgb(0,227,150)",
            hoverBackgroundColor: "rgb(0,227,150)",
            stack: 'total'
          },{
            label: 'Member',
            data: [1238, 553, 746, 884, 903],
            backgroundColor: "rgb(254,176,25)",
            hoverBackgroundColor: "rgb(254,176,25)",
            stack: 'total'
          }]
        }
      }
    );
    this.refresh();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.filterItems = this.filterItems;
    this.manageUserForBranchService.getAllValueOfUserInProjectByUserId(request)
    .pipe(
      finalize(() => {
        finishedCallback()
      })
    ).subscribe((rs: any) => {
      this.totalItems = rs.result.totalCount;
    })
  }

  protected delete(entity: any): void {
    throw new Error('Method not implemented.');
  }

}

export class projectDTO {
  projectId: number;
  projectCode: string;
  projectName: string;
  totalUser: number;
  memberCount: number;
  exposeCount: number;
  shadowCount: number;
}
