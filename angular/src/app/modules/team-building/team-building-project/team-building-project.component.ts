import { Component, Injector, OnInit } from "@angular/core";
import { MatDialog, MatSelectChange } from "@angular/material";
import { DomSanitizer } from "@angular/platform-browser";
import { TeamBuildingProjectService } from "@app/service/api/team-building-project.service";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { finalize } from "rxjs/operators";
import { DEFAULT_FILTER_VALUE, ProjectTeamBuildingDto, ProjectType, ProjectTypeEnum, SelectProjectIsAllowTeamBuildingDto } from "../const/const";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";

@Component({
  selector: "app-team-building-project",
  templateUrl: "./team-building-project.component.html",
  styleUrls: ["./team-building-project.component.css"],
})
export class TeamBuildingProjectComponent
  extends PagedListingComponentBase<ProjectTeamBuildingDto>
  implements OnInit
{
  public projectList: ProjectTeamBuildingDto[] = [];
  public projectType: ProjectTypeEnum = ProjectTypeEnum.All;
  public listProjectType: ProjectType[] = [
    { value: ProjectTypeEnum.All, title: "All" },
    { value: ProjectTypeEnum.TimeAndMaterials, title: "T&M" },
    { value: ProjectTypeEnum.FixedFee, title: "Fixed Frice" },
    { value: ProjectTypeEnum.NoneBillable, title: "Non-Bill" },
    { value: ProjectTypeEnum.ODC, title: "ODC" },
    { value: ProjectTypeEnum.Product, title: "Product" },
    { value: ProjectTypeEnum.Training, title: "Training" },
    { value: ProjectTypeEnum.NoSalary, title: "NoSalary" },
  ];
  public filter = {
    projectType: DEFAULT_FILTER_VALUE,
  };
  public allComplete: boolean = false;
  public listItemChange: SelectProjectIsAllowTeamBuildingDto[] = [];

  TeamBuilding_Project_SelectProjectTeamBuilding = PERMISSIONS_CONSTANT.TeamBuilding_Project_SelectProjectTeamBuilding;
  constructor(
    injector: Injector,
    private _teamBuildingProjectService: TeamBuildingProjectService,
    public _dialog: MatDialog,
    public sanitizer: DomSanitizer
  ) {
    super(injector);
  }
  protected delete(entity: ProjectTeamBuildingDto): void {
    throw new Error("Method not implemented.");
  }

  ngOnInit() {
    this.refresh();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.filterItems = [];
    if (this.searchText) {
      request.searchText = this.searchText;
    }
    if (this.projectType !== ProjectTypeEnum.All) {
      request.filterItems.push({
        propertyName: "projectType",
        value: this.projectType,
        comparison: 0,
      });
    }
    this.subscriptions.push(
      this._teamBuildingProjectService
        .getAll(request)
        .pipe(finalize(() => finishedCallback()))
        .subscribe((rs) => {
          this.projectList = rs.result.items;
          this.showPaging(rs.result, pageNumber);
          this.updateAllComplete();
        })
    );
  }

  public handleChangeProjectType(e: MatSelectChange) {
    this.projectType = e.value;
    this.refresh();
  }

  onClickBtnSave(){
    this.save();
  }

  save() {
    this._teamBuildingProjectService
    .selectIsAllowTeamBuilding(this.listItemChange)
    .subscribe((result) => {
      if(result)
        this.notify.success(this.l("Update Successfully!"));
      else
        this.notify.error(this.l("Update Fail!"));
      this.refresh();
    });
    this.listItemChange = [];
  }

  setAll(completed: boolean) {
    const self = this;
    this.allComplete = completed;
    if (this.projectList == null) {
      return;
    }
    this.projectList.forEach((project) => {
      const item = self.listItemChange.find(item => item.projectId == project.id);
      if(item){
        item.isAllowTeamBuilding = completed;
      }
      else{
        self.listItemChange.push({isAllowTeamBuilding: completed, projectId : project.id})
      }
    });
  }

  updateAllComplete() {
    if (this.projectList == null || this.projectList.length == 0) {
      return false;
    }

    this.allComplete = this.projectList != null && this.projectList.every(t => this.getIsAllowTeamBuilding(t));
  }

  handleSelectRequestInfoItem(index,$event){
    this.projectList[index].isAllowTeamBuilding = $event.checked;

    const item = this.listItemChange.find(item => item.projectId == this.projectList[index].id)
    if(item){
      item.isAllowTeamBuilding = $event.checked;
    }
    else{
      this.listItemChange.push({isAllowTeamBuilding: $event.checked, projectId : this.projectList[index].id})
    }
    this.updateAllComplete();
  }

  getIsAllowTeamBuilding(item){
    return this.listItemChange.find(c => c.projectId == item.id) ? this.listItemChange.find(c => c.projectId == item.id).isAllowTeamBuilding : item.isAllowTeamBuilding;
  }

  someComplete(): boolean {
    if (this.projectList == null || this.projectList.length == 0) {
      return false;
    }
    return this.projectList.filter(t => this.getIsAllowTeamBuilding(t)).length > 0 && !this.allComplete;
  }
}
