import { MatDialogRef } from "@angular/material";
import { Component, Injector, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { TeamBuildingPMService } from "@app/service/api/team-building-pm.service";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import * as _ from "lodash";
import { finalize } from "rxjs/operators";
import { MatDialog } from "@angular/material";
import { PmSendRequestComponent } from "./pm-send-request/pm-send-request.component";
import { LIST_MONTHS, PagedDetailDto, StatusTeamBuildingDetail, StatusTeamBuildingDetailEnum, TeamBuildingDetailPMDto } from "../const/const";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";

@Component({
  selector: "app-team-building-pm",
  templateUrl: "./team-building-pm.component.html",
  styleUrls: ["./team-building-pm.component.css"],
})
export class TeamBuildingPmComponent
  extends PagedListingComponentBase<TeamBuildingDetailPMDto>
  implements OnInit
{
  public searchText: string;
  public listYear = [];
  public year: number;
  public requesterEmail = [];
  public requesterEmailFilter = [];
  public selectedRequesterId: string = "";
  public requesterEmailSearch = "";
  public project = [];
  public projectFilter = [];
  public selectedProjectId: string = "";
  public projectSearch = "";
  public status: StatusTeamBuildingDetailEnum = StatusTeamBuildingDetailEnum.All;
  public listDetail: TeamBuildingDetailPMDto[] = [];
  public listStatus: StatusTeamBuildingDetail[] = [
    { value: StatusTeamBuildingDetailEnum.All, title: "All" },
    { value: StatusTeamBuildingDetailEnum.Open, title: "Open" },
    { value: StatusTeamBuildingDetailEnum.Requested, title: "Requested" },
    { value: StatusTeamBuildingDetailEnum.Done, title: "Done" },
  ];
  public inputRequest = {} as PagedDetailDto;

  public listMonth = LIST_MONTHS;
  public month: number = -1;

  TeamBuilding_DetailPM_CreateRequest = PERMISSIONS_CONSTANT.TeamBuilding_DetailPM_CreateRequest;

  constructor(
    injector: Injector,
    public _teamBuildingPMService: TeamBuildingPMService,
    public _dialog: MatDialog,
    public sanitizer: DomSanitizer
  ) {
    super(injector);
    var d = new Date().getFullYear();
    for (let i = d - 5; i <= d + 2; i++) {
      this.listYear.push(i);
    }
    this.year = d;
  }

  ngOnInit() {
    this.setDefaultFilter();
    this.refresh();
    this.getAllRequesterEmail();
    this.getAllProject();
  }

  getNameByValue(value) {
    for (let name in Object.keys(StatusTeamBuildingDetailEnum)) {
      if (StatusTeamBuildingDetailEnum[name] === value) {
        return name;
      }
    }
    return null; // value not found in the enum
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.filterItems = [];
    this.inputRequest = {
      gridParam: request,
      year: this.year,
      month: this.month,
      status: this.status == -1 ? null : this.status,
    };
    if (this.selectedRequesterId && this.selectedRequesterId != "null") {
      request.filterItems.push({
        comparison: 0,
        propertyName: "requesterId",
        value: this.selectedRequesterId,
      });
    }
    if (this.selectedRequesterId == "null") {
      request.filterItems.push({
        comparison: 0,
        propertyName: "requesterId",
        value: null,
      });
    }
    if (this.selectedProjectId && this.selectedProjectId != "null") {
      request.filterItems.push({
        comparison: 0,
        propertyName: "projectId",
        value: this.selectedProjectId,
      });
    }
    if (this.searchText) {
      request.searchText = this.searchText;
    }
    this._teamBuildingPMService
      .getAll(this.inputRequest)
      .pipe(finalize(() => finishedCallback()))
      .subscribe((rs) => {
        this.listDetail = rs.result.items;
        this.showPaging(rs.result, pageNumber);
      });
  }

  getAllRequesterEmail() {
    this._teamBuildingPMService
      .getAllRequesterEmailAddressInTeamBuildingDetailPM()
      .subscribe((data) => {
        this.requesterEmail = this.requesterEmailFilter = data.result;
      });
  }

  handleSearch() {
    const textSearch = this.requesterEmailSearch.toLowerCase().trim();
    if (textSearch) {
      this.requesterEmail = this.requesterEmailFilter.filter((item) =>
        item.requesterEmailAddress.toLowerCase().trim().includes(textSearch)
      );
    } else {
      this.requesterEmail = _.cloneDeep(this.requesterEmailFilter);
    }
  }
  pmSendReQuest(): void {
    const dialogRef = this._dialog.open(PmSendRequestComponent, {
      disableClose: true,
      width: window.innerWidth >= 1000 ? "1000px" : "90%",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.refresh();
      }
    });
  }
  getAllProject() {
    this._teamBuildingPMService
      .getAllProjectInTeamBuildingDetailPM()
      .subscribe((data) => {
        this.project = this.projectFilter = data.result;
      });
  }

  handleSearchProject() {
    const textSearch = this.projectSearch.toLowerCase().trim();
    if (textSearch) {
      this.project = this.projectFilter.filter((item) =>
        item.projectName.toLowerCase().trim().includes(textSearch)
      );
    } else {
      this.project = _.cloneDeep(this.projectFilter);
    }
  }

  setDefaultFilter() {
    this.year = new Date().getFullYear();
    this.status = -1;
  }

  handleFilterYear(e) {
    this.year = e.value;
    this.refresh();
  }

  handleFilterStatus(e) {
    this.status = e.value;
    this.refresh();
  }

  handleFilterMonth(m) {
    this.month = m.value;
    this.refresh();
  }

  protected delete(entity: TeamBuildingDetailPMDto): void {
    throw new Error("Method not implemented.");
  }
}

