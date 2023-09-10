import { Options } from "@angular-slider/ngx-slider";
import { Component, Injector, OnInit } from "@angular/core";
import { MatDialog } from "@angular/material";
import { ActivatedRoute } from "@angular/router";
import { APP_CONSTANT } from "@app/constant/api.constants";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { BranchService } from "@app/service/api/branch.service";
import { PositionDto } from "@app/service/api/model/position-dto";
import {
  Action,
  ActionEnum,
  EnumLevel,
  EnumSort,
  EnumSortName,
  EnumStatus,
  EnumUserType,
  PagedRequestRetroDetailDto,
  RetroDetailBranch,
  RetroDetailCreateEditDto,
  RetroDetailDialogData,
  RetroDetailDto,
  RetroDetailFilter,
  RetroDetailMember,
  RetroDetailProject,
  RetroDetailSort,
} from "@app/service/api/model/retro-detail-dto";
import { PositionService } from "@app/service/api/position.service";
import { RetroDetailService } from "@app/service/api/retro-detail.service";
import { ActionDialog } from "@shared/AppEnums";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import * as FileSaver from "file-saver";
import { finalize } from "rxjs/operators";
import { CreateEditRetroDetailComponent } from "./create-edit-retro-detail/create-edit-retro-detail.component";
import { ImportRetroDetailComponent } from "./import-retro-detail/import-retro-detail.component";

@Component({
  selector: "app-retro-detail",
  templateUrl: "./retro-detail.component.html",
  styleUrls: ["./retro-detail.component.css"],
})
export class RetroDetailComponent
  extends PagedListingComponentBase<RetroDetailDto>
  implements OnInit
{
  //TODO Permission
  VIEWALLTEAM = PERMISSIONS_CONSTANT.RetroDetail_ViewAllTeam;
  VIEWMYTEAM = PERMISSIONS_CONSTANT.RetroDetail_ViewMyTeam;
  VIEWLEVEL = PERMISSIONS_CONSTANT.RetroDetail_ViewLevel;
  ADD = PERMISSIONS_CONSTANT.RetroDetail_AddEmployeeMyTeam;
  ADDTEAM = PERMISSIONS_CONSTANT.RetroDetail_AddEmployeeAllTeam;
  EDIT = PERMISSIONS_CONSTANT.RetroDetail_EditEmployee;
  DELETE = PERMISSIONS_CONSTANT.RetroDetail_DeleteEmployee;
  IMPORT = PERMISSIONS_CONSTANT.RetroDetail_Import;
  DOWNLOADTEMPLATE = PERMISSIONS_CONSTANT.RetroDetail_DownloadTemplate;
  EXPORT = PERMISSIONS_CONSTANT.RetroDetail_Export;
  listRetroDetail = [] as RetroDetailDto[];

  private retroId: number;
  private requestExport: PagedRequestRetroDetailDto & { retroId: number };

  minValue: number = 0;
  maxValue: number = 5;
  options: Options = {
    floor: 0,
    ceil: 5,
    step: 0.1,
  };

  public listUserType: RetroDetailFilter[] = [
    { id: EnumUserType.Staff, name: "Staff" },
    { id: EnumUserType.Internship, name: "Internship" },
    { id: EnumUserType.Collaborator, name: "Collaborator" },
  ];
  public listUserTypeSelected: number[] = [];

  public listLevel: RetroDetailFilter[] = [
    { id: EnumLevel.Intern_0, name: "Intern_0" },
    { id: EnumLevel.Intern_1, name: "Intern_1" },
    { id: EnumLevel.Intern_2, name: "Intern_2" },
    { id: EnumLevel.Intern_3, name: "Intern_3" },
    { id: EnumLevel.FresherMinus, name: "Fresher-" },
    { id: EnumLevel.Fresher, name: "Fresher" },
    { id: EnumLevel.FresherPlus, name: "Fresher+" },
    { id: EnumLevel.JuniorMinus, name: "Junior-" },
    { id: EnumLevel.Junior, name: "Junior" },
    { id: EnumLevel.JuniorPlus, name: "Junior+" },
    { id: EnumLevel.MiddleMinus, name: "Middle-" },
    { id: EnumLevel.Middle, name: "Middle" },
    { id: EnumLevel.MiddlePlus, name: "Middle+" },
    { id: EnumLevel.SeniorMinus, name: "Senior-" },
    { id: EnumLevel.Senior, name: "Senior" },
    { id: EnumLevel.SeniorPlus, name: "Senior+" },
  ];
  public listLevelSelected: number[] = [];

  public listPosition: PositionDto[] = [];
  public listPositionSelected: number[] = [];

  public listBranch: RetroDetailBranch[] = [];
  public listBranchSelected: number[] = [];

  public listProject: RetroDetailProject[] = [];
  public listProjectSelected: number[] = [];

  public listAction: Action[] = [
    { icon: ActionEnum.Edit, title: "Edit" },
    { icon: ActionEnum.Delete, title: "Delete" },
  ];

  public sortByName: RetroDetailSort = {
    name: EnumSortName.FullName,
    value: EnumSort.NotArranged,
  };

  public sortByPoint: RetroDetailSort = {
    name: EnumSortName.Point,
    value: EnumSort.NotArranged,
  };

  public status: EnumStatus;
  public title: string;
  public requestRetroDetail: PagedRequestRetroDetailDto;
  constructor(
    private retroDetailService: RetroDetailService,
    private positionService: PositionService,
    private branchService: BranchService,
    private dialog: MatDialog,
    private activatedRoute: ActivatedRoute,
    injector: Injector
  ) {
    super(injector);
    this.status = Number(activatedRoute.snapshot.queryParams.status);
    this.retroId = Number(activatedRoute.snapshot.queryParams.retroId);
    this.title = `Retro result: ${activatedRoute.snapshot.queryParams.name}`;
  }

  ngOnInit() {
    this.initValueSelected();
  }

  initValueSelected() {
    this.listUserTypeSelected = [];
    this.listLevelSelected = [];
    this.listPositionSelected = [];
    this.listBranchSelected = [];
    this.listProjectSelected = [];
    this.minValue = 0;
    this.maxValue = 5;
    // for (let i = 0; i < this.listLevel.length; i++) {
    //   this.listLevelSelected.push(this.listLevel[i].id);
    // }
    // for (let i = 0; i < this.listUserType.length; i++) {
    //   this.listUserTypeSelected.push(this.listUserType[i].id);
    // }
    this.getProjectPMRetroResult();

    this.branchService.getAllBranchFilter(false).subscribe((value) => {
      this.listBranch = value.result;
      //this.listBranchSelected = this.listBranch.map((item) => item.id);
    });
    this.positionService.GetAllPosition().subscribe((value) => {
      this.listPosition = value.result.items;
      //this.listPositionSelected = this.listPosition.map((item) => item.id);
      //this.refresh();
    });
    this.getDataPage(1);
  }
  public getProjectPMRetroResult(){
    this.retroDetailService.getProjectPMRetroResult(this.retroId).subscribe((value) => {
      this.listProject = value.result;
      this.listProjectSelected = [];
    });
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this.requestRetroDetail = {
      gridParam: {
        ...request,
      },
    };
    this.handleFilter(this.requestRetroDetail);
    this.requestExport = { ...this.requestRetroDetail, ...this.requestExport };
    this.retroDetailService
      .getAll(this.requestRetroDetail, this.retroId)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.listRetroDetail = result.result.items;
        for (let i = 0; i < this.listRetroDetail.length; i++) {
          this.listRetroDetail[i].hideNote = false;
        }
        this.showPaging(result.result, pageNumber);
      });
  }

  handleFilter(request: PagedRequestRetroDetailDto) {
    request.gridParam.filterItems = [];
    request.gridParam.sort = "";
    request.gridParam.sortDirection = 0;
    if (this.searchText) {
      request.gridParam.searchText = this.searchText;
    }
    if (this.sortByName.value !== EnumSort.NotArranged) {
      request.gridParam.sort = this.sortByName.name;
      request.gridParam.sortDirection = this.sortByName.value;
    }
    if (this.sortByPoint.value !== EnumSort.NotArranged) {
      request.gridParam.sort = this.sortByPoint.name;
      request.gridParam.sortDirection = this.sortByPoint.value;
    }
    request.usertypes = [...this.listUserTypeSelected];
    request.userlevels = [...this.listLevelSelected];
    request.positionIds = [...this.listPositionSelected];
    request.projecIds = [...this.listProjectSelected];
    request.branchIds = [...this.listBranchSelected];
    request.leftPoint = this.minValue;
    request.rightPoint = this.maxValue;
  }

  protected delete(item: RetroDetailDto): void {
    abp.message.confirm(
      "Delete user '" + item.fullName + "'?",
      (result: boolean) => {
        if (result) {
          this.retroDetailService.delete(item.id).subscribe(() => {
            abp.notify.info("Deleted user " + item.fullName);
            this.getProjectPMRetroResult();
            this.refresh();
          });
        }
      }
    );
  }

  onFilter() {
    this.actionChangeSelected();
  }

  handleClear() {
    this.listUserTypeSelected = [];
    this.listLevelSelected = [];
    this.listPositionSelected = [];
    this.listBranchSelected = [];
    this.listProjectSelected = [];
    this.minValue = 0;
    this.maxValue = 5;
    this.getDataPage(1);
  }

  handleShowAction() {
    return (
      (this.permission.isGranted(this.DELETE) ||
        this.permission.isGranted(this.EDIT)) &&
      this.status === 0
    );
  }

  getAvatar(member) {
    if (member.internFullPath) {
      return member.internFullPath;
    }
    return "assets/images/undefine.png";
  }

  changeStatusNote(item) {
    item.hideNote = !item.hideNote;
  }

  create(): void {
    let item = {} as RetroDetailCreateEditDto;
    item.retroId = this.retroId;
    this.showDialog(item, ActionDialog.CREATE);
  }

  edit(retroDetail: RetroDetailCreateEditDto): void {
    this.showDialog(retroDetail, ActionDialog.EDIT);
  }

  actionChangeSelected() {
    this.handleFilter(this.requestExport);
    this.retroDetailService
      .getAll(this.requestExport, this.retroId)
      .subscribe((result) => {
        this.listRetroDetail = result.result.items;
        for (let i = 0; i < this.listRetroDetail.length; i++) {
          this.listRetroDetail[i].hideNote = false;
        }
        this.showPaging(result.result, 1);
      });
  }

  onChangeListUserTypes(event) {
    this.listUserTypeSelected = event;
    this.actionChangeSelected();
  }

  onChangeListLevel(event) {
    this.listLevelSelected = event;
    this.actionChangeSelected();
  }

  onChangeListPosition(event) {
    this.listPositionSelected = event;
    this.actionChangeSelected();
  }

  onChangeListBranch(event) {
    this.listBranchSelected = event;
    this.actionChangeSelected();
  }

  onChangeListProject(event) {
    this.listProjectSelected = event;
    this.actionChangeSelected();
  }

  getLevelById(id: number) {
    for (let i = 0; i < this.listLevel.length; i++) {
      if (id === this.listLevel[i].id) {
        return this.listLevel[i].name;
      }
    }
  }

  handleSortName() {
    this.sortByPoint.value = EnumSort.NotArranged;
    if (this.sortByName.value === EnumSort.NotArranged) {
      this.sortByName.value = EnumSort.ASC;
    } else if (this.sortByName.value === EnumSort.ASC) {
      this.sortByName.value = EnumSort.DEC;
    } else {
      this.sortByName.value = EnumSort.NotArranged;
    }
    this.refresh();
  }

  handleSortPoint() {
    this.sortByName.value = EnumSort.NotArranged;
    if (this.sortByPoint.value === EnumSort.NotArranged) {
      this.sortByPoint.value = EnumSort.ASC;
    } else if (this.sortByPoint.value === EnumSort.ASC) {
      this.sortByPoint.value = EnumSort.DEC;
    } else {
      this.sortByPoint.value = EnumSort.NotArranged;
    }
    this.refresh();
  }

  handleAction(type: ActionEnum, item: RetroDetailDto) {
    if (type === ActionEnum.Edit) {
      const itemEdit = {
        note: item.note,
        point: item.point,
        retroId: this.retroId,
        projectId: item.projectId,
        positionId: item.positionId,
        id: item.id,
        userId: item.userId,
        userName: item.fullName,
        branchId: item.branchId,
        userType: item.type,
        userLevel: item.level,
        projectName: item.projectName,
        pmId: item.pmId
      } as RetroDetailCreateEditDto;
      // this.retroDetailService
      //   .getUserByProjectId(itemEdit.projectId)
      //   .subscribe((value) => {
      //     const listUser = value.result as RetroDetailMember[];
      //     for (let i = 0; i < listUser.length; i++) {
      //       if (listUser[i].fullName === item.fullName) {
      //         itemEdit.userId = listUser[i].userId;
      //         itemEdit.userName = listUser[i].fullName;
      //         console.log(itemEdit.userName);
      //         break;
      //       }
      //     }
      //   }, error => abp.message.error(error.message));
      this.edit(itemEdit);
    } else {
      this.delete(item);
    }
  }

  showDialog(
    retroDetail: RetroDetailCreateEditDto,
    action: ActionDialog
  ): void {
    const {
      id,
      userId,
      projectId,
      positionId,
      point,
      note,
      retroId,
      userName,
      branchId,
      userLevel,
      userType,
      projectName,
      pmId
    } = retroDetail;
    let item = {
      id,
      userId,
      projectId,
      positionId,
      point,
      note,
      retroId,
      userName,
      branchId,
      userLevel,
      userType,
      projectName,
      pmId
    } as RetroDetailCreateEditDto;
    const dialogRef = this.dialog.open(CreateEditRetroDetailComponent, {
      data: {
        item: item,
        action: action,
      } as RetroDetailDialogData,
      disableClose: true,
      width: "900px",
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        this.getProjectPMRetroResult();
        const firstPage = 1;
        const pageNumber = retroDetail.id == null ? firstPage : this.pageNumber;
        this.getDataPage(pageNumber);
      }
    });
  }
  public onImportFile() {
    this.dialog
      .open(ImportRetroDetailComponent, {
        data: {
          retroId: this.retroId,
        },
        disableClose: true,
        width: "450px",
      })
      .afterClosed()
      .subscribe(() => {
        this.getProjectPMRetroResult();
        this.refresh();
      });
  }

  private convertFile(fileData) {
    var buf = new ArrayBuffer(fileData.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != fileData.length; ++i)
      view[i] = fileData.charCodeAt(i) & 0xff;
    return buf;
  }

  public downloadTemplate() {
    this.retroDetailService.downloadTemplate().subscribe((rs) => {
      const file = new Blob([this.convertFile(atob(rs.result.base64))], {
        type: "application/vnd.ms-excel;charset=utf-8",
      });
      FileSaver.saveAs(file, "TemplateImportRetro.xlsx");
    });
  }

  public handleExport() {
    this.requestExport.retroId = this.retroId;
    this.retroDetailService
      .ExportRetroResult(this.requestRetroDetail, this.retroId)
      .subscribe((rs) => {
        const file = new Blob([this.convertFile(atob(rs.result.base64))], {
          type: "application/vnd.ms-excel;charset=utf-8",
        });
        FileSaver.saveAs(file, "ExportRetro.xlsx");
      });
  }
}
