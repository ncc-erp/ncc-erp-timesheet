import { AppComponentBase } from "@shared/app-component-base";
import {
  LIST_MONTHS,
  PMRequestDto,
  RequestDto,
  SelectTeamBuildingDetailDto,
  SelectUserOtherProjectDto,
  StatusTeamBuildingDetail,
  StatusTeamBuildingDetailEnum,
} from "../const/const";
import { COMMA, ENTER } from "@angular/cdk/keycodes";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import * as _ from "lodash";
import { Injector } from "@angular/core";
import { TeamBuildingPMService } from "@app/service/api/team-building-pm.service";
import { MatChipInputEvent } from "@angular/material";

export abstract class CreateEditRequestComponentBase extends AppComponentBase {

  public requestInfo: RequestDto[] = [];
  public requestAdding: RequestDto[] = [];
  public originalList: RequestDto[] = [];

  public lastRemainMoney: number = 0;
  public totalMoney: number = 0;

  public selectedCheckboxCount: number = 0;
  public isLoading: boolean = false;
  public saving: boolean = false;

  public listSelectedItem: SelectTeamBuildingDetailDto[] = [];

  public listUserIdNotProjectSelected: number[] = [];
  public isFirst = true;

  public listUserNotInProject = [];
  public allComplete: boolean = false;

  public selectedFiles: FileList;
  public uploadedFiles: string[] = [];
  public fileUploadList = [];
  public fileReader = new FileReader();
  public visible = true;
  public selectable = true;
  public removable = true;
  public addOnBlur = true;
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];
  public hasUrl: boolean = false;

  public project = [];
  public projectFilter = [];
  public selectedProjectId = 0;
  public projectSearch = "";

  public listBranch: BranchDto[] = [];
  public branchSearch: "";
  public listBranchFilter: BranchDto[];
  public branchId = 0;

  public listMonth = LIST_MONTHS;
  public month: number = null;

  public isShowBtnAddUser = false;
  public listBranchUserOtherProject: BranchDto[] = [];
  public branchSearchUserOtherProject: "";
  public listBranchFilterUserOtherProject: BranchDto[];
  public branchIdUserOtherProject = "";

  public listSelectedItemUserOtherProject: SelectUserOtherProjectDto[] = [];
  public allCompleteUserOtherProject: boolean = false;
  public searchText: string = "";

  public listStatus: StatusTeamBuildingDetail[] = [
    { value: StatusTeamBuildingDetailEnum.All, title: "All" },
    { value: StatusTeamBuildingDetailEnum.Open, title: "Open" },
    { value: StatusTeamBuildingDetailEnum.Requested, title: "Requested" },
    { value: StatusTeamBuildingDetailEnum.Done, title: "Done" },
  ];

  public currentSortColumn: string = "transactionDate";
  public sortDirection: number = 0;
  public iconSort: string = "";

  public isCreateRequest: boolean;

  constructor(
    injector: Injector,
    public teamBuildingPMService: TeamBuildingPMService
  ) {
    super(injector);
  }

  getIsSelected(item) {
    return this.listSelectedItem.find((c) => c.id == item.id)
      ? this.listSelectedItem.find((c) => c.id == item.id).selected
      : item.selected;
  }

  setAll(completed: boolean) {
    const self = this;
    this.allComplete = completed;
    if (this.requestInfo == null) {
      return;
    }
    this.requestInfo.forEach((teamBuildingDetail) => {
      const item = self.listSelectedItem.find(
        (item) => item.id == teamBuildingDetail.id
      );
      if (item) {
        item.selected = completed;
      } else {
        self.listSelectedItem.push({
          selected: completed,
          id: teamBuildingDetail.id,
          money: teamBuildingDetail.money,
          status: teamBuildingDetail.status,
        });
      }
    });

    this.getTotalMoney();
    this.checkSelectedCheckbox();
  }

  someComplete(): boolean {
    if (this.requestInfo == null || this.requestInfo.length == 0) {
      return false;
    }

    this.getTotalMoney();
    return (
      this.requestInfo.filter((t) => this.getIsSelected(t)).length > 0 &&
      !this.allComplete
    );
  }

  // hàm check lựa chọn bản ghi
  checkSelectedCheckbox() {
    this.selectedCheckboxCount = this.listSelectedItem.filter(
      (x) => x.selected
    ).length;
  }

  getTotalMoney() {
    this.totalMoney = 0;
    this.listSelectedItem.forEach((x) => {
      if (this.getIsSelected(x) && x.status != StatusTeamBuildingDetailEnum.Requested) {
        this.totalMoney += x.money;
      }
    });
    return this.totalMoney;
  }

  // Hàm xử lý file, URL
  getShortNameOfUrlImage(url: string) {
    return url.slice(0, 10) + "..." + url.slice(url.lastIndexOf("."));
  }

  onFileSelected(event) {
    this.selectedFiles = event.target.files;
    for (let i = 0; i < this.selectedFiles.length; i++) {
      this.fileReader.readAsDataURL(this.selectedFiles.item(i));
    }
  }

  getAllProject() {
    this.teamBuildingPMService
      .getAllProjectInTeamBuildingDetailPM()
      .subscribe((data) => {
        this.project = this.projectFilter = data.result;
      });
  }

  getListBranch() {
    this.teamBuildingPMService.getAllRequestByBranch().subscribe((res) => {
      this.listBranch = this.listBranchFilter = res.result;
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

  handleSearchBranch() {
    const textSearch = this.branchSearch.toLowerCase().trim();
    if (textSearch) {
      this.listBranch = this.listBranchFilter.filter((item) =>
        item.name.toLowerCase().trim().includes(textSearch)
      );
    } else {
      this.listBranch = _.cloneDeep(this.listBranchFilter);
    }
  }

  handleFilterMonth(m) {
    this.month = m.value;
  }

  //Hàm Sort theo các trường
  handleSortByColumn(columnName) {
    if (this.currentSortColumn !== columnName) {
      this.sortDirection = -1;
    }
    this.currentSortColumn = columnName;
    this.sortDirection++;
    if (this.sortDirection > 1) {
      this.iconSort = "";
      this.sortDirection = -1;
    }

    if (this.sortDirection == 0 || this.sortDirection == 1) {
      this.iconSort =
        this.sortDirection == 1
          ? "fas fa-sort-amount-down"
          : "fas fa-sort-amount-up";

      switch (columnName) {
        case "employeeFullName":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.employeeEmailAddress.localeCompare(a.employeeEmailAddress)
              : a.employeeEmailAddress.localeCompare(b.employeeEmailAddress);
          });
          break;
        case "branch":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.branchName.localeCompare(a.branchName)
              : a.branchName.localeCompare(b.branchName);
          });
          break;
        case "project":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.projectName.localeCompare(a.projectName)
              : a.projectName.localeCompare(b.projectName);
          });
          break;
        case "month":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.applyMonth
                  .split("-")[1]
                  .split("T")[0]
                  .localeCompare(a.applyMonth.split("-")[1].split("T")[0])
              : a.applyMonth
                  .split("-")[1]
                  .split("T")[0]
                  .localeCompare(b.applyMonth.split("-")[1].split("T")[0]);
          });
          break;
        case "status":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.status - a.status
              : a.status - b.status;
          });
          break;
      }
    } else {
      this.iconSort = "fas fa-sort";
      this.requestInfo = [...this.originalList];
    }
  }

  toggleShowAddUserDialog(){
    this.isShowBtnAddUser = !this.isShowBtnAddUser;
  }
}
