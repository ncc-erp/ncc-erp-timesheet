import { AddEmployeeComponent } from "./add-employee/add-employee.component";
import { ActionDialog } from "@shared/AppEnums";
import { GenerateDataComponent } from "./generate-data/generate-data.component";
import { MatDialog } from "@angular/material";
import { Component, Injector, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { TeamBuildingHRService } from "@app/service/api/team-building-hr.service";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import * as _ from "lodash";
import { finalize } from "rxjs/operators";
import { CreateEditTeambuildingDetailDto, EditMoneyTeamBuildingDetailDialogData, EditMoneyTeamBuildingDetailDto, LIST_MONTHS, PagedDetailDto, StatusTeamBuildingDetail, StatusTeamBuildingDetailEnum, TeamBuildingDetailDto } from "../const/const";
import { EditMoneyComponent } from "./edit-money/edit-money.component";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";

@Component({
  selector: "app-team-building-hr",
  templateUrl: "./team-building-hr.component.html",
  styleUrls: ["./team-building-hr.component.css"],
})
export class TeamBuildingHrComponent
  extends PagedListingComponentBase<TeamBuildingDetailDto>
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
  public listDetail: TeamBuildingDetailDto[] = [];
  public listStatus: StatusTeamBuildingDetail[] = [
    { value: StatusTeamBuildingDetailEnum.All, title: "All" },
    { value: StatusTeamBuildingDetailEnum.Open, title: "Open" },
    { value: StatusTeamBuildingDetailEnum.Requested, title: "Requested" },
    { value: StatusTeamBuildingDetailEnum.Done, title: "Done" },
  ];
  public inputRequest = {} as PagedDetailDto;

  public listMonth = LIST_MONTHS;
  public month: number = -1;

  TeamBuilding_DetailHR_GenerateData = PERMISSIONS_CONSTANT.TeamBuilding_DetailHR_GenerateData;
  TeamBuilding_DetailHR_Management = PERMISSIONS_CONSTANT.TeamBuilding_DetailHR_Management;

  constructor(
    injector: Injector,
    private _teamBuildingHRService: TeamBuildingHRService,
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
    this.getAllRequesterEmail();
    this.getAllProject();
    this.refresh();
  }
  getNameByValue(value) {
    for (let name in Object.keys(StatusTeamBuildingDetailEnum)) {
      if (StatusTeamBuildingDetailEnum[name] === value) {
        return name;
      }
    }
    return null; // value not found in the enum
  }
  generateData(): void {
    const dialogRef = this._dialog.open(GenerateDataComponent, {
      disableClose: true,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        if (result.month) {
          this.isTableLoading = true;
          this._teamBuildingHRService
            .addDataToTeamBuildingDetail(result)
            .subscribe(
              (res) => {
                abp.notify.success("Generated success");
                this.refresh();
                this.isTableLoading = false;
              },
              () => (this.isTableLoading = false)
            );
        } else {
          var today = new Date();
          var time = {
            year: today.getFullYear(),
            month: today.getMonth() + 1,
          };
          this.isTableLoading = true;
          this._teamBuildingHRService
            .addDataToTeamBuildingDetail(time)
            .subscribe(
              (res) => {
                abp.notify.success("Generated success");
                this.refresh();
                this.isTableLoading = false;
              },
              () => (this.isTableLoading = false)
            );
          this.refresh();
        }
      }
    });
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
    this.subscriptions.push(
      this._teamBuildingHRService
        .getAll(this.inputRequest)
        .pipe(finalize(() => finishedCallback()))
        .subscribe((rs) => {
          this.listDetail = rs.result.items;
          this.showPaging(rs.result, pageNumber);
        })
    );
  }

  getAllRequesterEmail() {
    this._teamBuildingHRService
      .getAllRequesterEmailAddressInTeamBuildingDetail()
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

  getAllProject() {
    this._teamBuildingHRService
      .getAllProjectInTeamBuildingDetail()
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

  handleFilterMonth(m) {
    this.month = m.value;
    this.refresh();
  }

  handleFilterStatus(e) {
    this.status = e.value;
    this.refresh();
  }

  protected delete(item: TeamBuildingDetailDto): void {
    abp.message.confirm(
      "Delete record '" + item.employeeFullName + "'?",
      (result: boolean) => {
        if (result) {
          this._teamBuildingHRService.delete(item.id).subscribe(() => {
            abp.notify.info("Deleted record " + item.employeeFullName);
            this.refresh();
          });
        }
      }
    );
  }
  create(): void {
    const dig = this._dialog.open(AddEmployeeComponent, {
      width: "700px",
    });
    dig.afterClosed().subscribe((rs) => {
      if (rs) {
        this.refresh();
      }
    });
  }

  edit(teambuildingDetail: EditMoneyTeamBuildingDetailDto): void {
    this.showDialogEditMoney(teambuildingDetail, ActionDialog.EDIT);
  }

  showDialogEditMoney(teambuildingDetail: EditMoneyTeamBuildingDetailDto, action: ActionDialog): void {
    const { id, money } = teambuildingDetail;
    let item = {
      id,
      money
    } as EditMoneyTeamBuildingDetailDto;
    const dialogRef = this._dialog.open(EditMoneyComponent, {
      data: {
        item: item,
        action: action,
      } as EditMoneyTeamBuildingDetailDialogData,
      disableClose: true,
      width: "500px",
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        const firstPage = 1;
        const pageNumber = teambuildingDetail.id == null ? firstPage : this.pageNumber;
        this.getDataPage(pageNumber);
      }
    });
  }
}
