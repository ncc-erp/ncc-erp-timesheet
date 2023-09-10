import { Component, Inject, Injector, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import {
  MatDialogRef,
  MatSelect,
  MatSelectChange,
  MAT_DIALOG_DATA,
} from "@angular/material";
import { ActivatedRoute } from "@angular/router";
import { PositionDto } from "@app/service/api/model/position-dto";
import {
  EnumLevel,
  EnumUserType,
  RetroDetailCreateEditDto,
  RetroDetailDialogData,
  RetroDetailFilter,
  RetroDetailMember,
  RetroDetailPm,
  RetroDetailProject,
} from "@app/service/api/model/retro-detail-dto";
import { PositionService } from "@app/service/api/position.service";
import { RetroDetailService } from "@app/service/api/retro-detail.service";
import { AppComponentBase } from "@shared/app-component-base";
import { ActionDialog } from "@shared/AppEnums";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { BranchService } from "@app/service/api/branch.service";

@Component({
  selector: "app-create-edit-retro-detail",
  styleUrls: ["./create-edit-retro-detail.component.css"],
  templateUrl: "./create-edit-retro-detail.component.html",
})
export class CreateEditRetroDetailComponent
  extends AppComponentBase
  implements OnInit
{
  @ViewChild("select") select: any;

  public retroDetail = {} as RetroDetailCreateEditDto;
  public title: string;
  public isSaving: boolean = false;
  public action: ActionDialog;
  public retroId: number;
  public temUserId: number;
  public temPmId: number;
  public defaultPMId: number = 0;

  public formData = this.fb.group({
    userId: [undefined, Validators.required],
    projectId: [undefined, Validators.required],
    positionId: [undefined, Validators.required],
    point: [undefined, [Validators.min(0), Validators.max(5)]],
    note: [""],
    userName: [""],
    branchId: [undefined],
    userType: [undefined],
    userLevel: [undefined],
    pmId: [undefined, Validators.required],
  });

  public listProject: RetroDetailProject[] = [];
  public listProjectBySearch: RetroDetailProject[] = [];
  public listMember: RetroDetailMember[] = [];
  public listMemberBySearch: RetroDetailMember[] = [];
  public listPosition: PositionDto[] = [];
  public listBranch: BranchDto[] = [];
  public listPm: RetroDetailPm[] = [];
  public listPmBySearch: RetroDetailPm[] = [];

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

  public searchText = "";
  public searchUser = "";
  public searchPm = "";

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: RetroDetailDialogData,
    injector: Injector,
    private _dialogRef: MatDialogRef<CreateEditRetroDetailComponent>,
    private fb: FormBuilder,
    private retroDetailService: RetroDetailService,
    private positionService: PositionService,
    private route: ActivatedRoute,
    private branchService: BranchService
  ) {
    super(injector);
    this.retroDetail = this.data.item;
    this.action = this.data.action;
    this.retroId = route.snapshot.queryParams.retroId;
  }

  ngOnInit(): void {
    for (let i = 0; i < this.listLevel.length; i++) {
      this.listLevelSelected.push(this.listLevel[i].id);
    }
    for (let i = 0; i < this.listUserType.length; i++) {
      this.listUserTypeSelected.push(this.listUserType[i].id);
    }
    this.getAllBranch();
    this.setTitleDialog();
    this.retroDetailService.getAllProject().subscribe((value) => {
      this.listProject = value.result;
      this.listProjectBySearch = value.result;
    });
    this.positionService.GetAllPosition().subscribe((value) => {
      this.listPosition = value.result.items;
    });
    if (this.action === ActionDialog.EDIT) {
      this.formData.patchValue({
        projectId: this.retroDetail.projectId,
        positionId: this.retroDetail.positionId,
        point: this.retroDetail.point.toFixed(1),
        note: this.retroDetail.note,
        userId: this.retroDetail.userId,
        userName: this.retroDetail.userName,
        branchId: this.retroDetail.branchId,
        userType: this.retroDetail.userType,
        userLevel: this.retroDetail.userLevel,
      });
    }
    if (this.data.action != ActionDialog.EDIT) this.getAllUser();

    if(this.data.action == ActionDialog.EDIT){
      this.onEditInit();
    }

  }
  onEditInit(){
    this.getAllPm(this.data.item.projectId);
    this.retroDetail.pmId = this.data.item.pmId;
  }

  setTitleDialog() {
    if (this.action === ActionDialog.CREATE) {
      this.title = "New employee";
    } else {
      this.title = "Update employee";
    }
  }
  isShowSelectorMember() {
    return this.data.action == ActionDialog.CREATE;
  }

  getAllUser(projectId? : number) {
    this.retroDetailService
      .getAllUsers(this.retroDetail.retroId, projectId)
      .subscribe((value) => {
        this.listMember = value.result;
        this.listMemberBySearch = value.result;
        if(!value.result.filter(s => s.userId == this.temUserId).length){
          this.formData.controls.userId.setValue(undefined);
        }
        else{
          this.formData.controls.userId.setValue(this.temUserId);
        }
      });
  }

  getAllPm(projectId? : number) {
    this.retroDetailService
      .getAllPms(this.retroDetail.retroId, projectId)
      .subscribe((value) => {
        this.listPm = value.result;
        const defaultPM = this.listPm.find(x => {
          return x.isDefault;
        });
        if(defaultPM) this.temPmId = defaultPM.pmId;
        this.listPmBySearch = value.result;
        this.defaultPMId = this.temPmId;
      });
  }

  handleOpenSelectProject() {
    if (this.searchText && this.listProjectBySearch.length === 0) {
      this.searchText = "";
      this.listProjectBySearch = this.listProject;
    }
  }

  handleOpenSelectMember() {
    if (this.searchUser && this.listMemberBySearch.length === 0) {
      this.searchUser = "";
      this.listMemberBySearch = this.listMember;
    }
  }

  handleOpenSelectPm() {
    if (this.searchPm && this.listPmBySearch.length === 0) {
      this.searchPm = "";
      this.listPmBySearch = this.listPm;
    }
  }

  onSave() {
    this.retroDetail = { ...this.formData.value, id: this.retroDetail.id };
    this.retroDetail.point = Number(this.retroDetail.point);
    this.retroDetail.retroId = Number(this.retroId);
    if (this.action == ActionDialog.CREATE) {
      this.doCreate();
      return;
    }
    this.doUpdate();
  }

  public doCreate() {
    this.isSaving = true;
    const payload = {
      projectId: this.retroDetail.projectId,
      positionId: this.retroDetail.positionId,
      point: this.retroDetail.point.toFixed(1),
      note: this.retroDetail.note,
      userId: this.retroDetail.userId,
      retroId: this.retroDetail.retroId,
      pmId: this.retroDetail.pmId,
    };
    this.retroDetailService.create(payload).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Add Employee successfully"));
          this.close(1);
        }
      },
      (err) => {
        this.isSaving = false;
      }
    );
  }

  doUpdate() {
    this.isSaving = true;
    this.retroDetailService.update(this.retroDetail).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Update Employee successfully"));
          this.close(1);
        }
      },
      (err) => {
        this.isSaving = false;
      }
    );
  }
  getAllBranch() {
    this.branchService.getAllBranchFilter(false).subscribe((value) => {
      this.listBranch = value.result;
    });
  }

  handleChangeSearchText(e: Event) {
    this.searchText = (e.target as HTMLInputElement).value;
    this.listProjectBySearch = this.listProject;
    if (this.searchText) {
      this.listProjectBySearch = this.listProjectBySearch.filter((item) =>
        item.name.toLowerCase().includes(this.searchText.toLowerCase().trim())
      );
    }
  }

  isEdit() {
    return this.data.action == ActionDialog.EDIT;
  }
  handleSearchMember(e: Event) {
    this.searchUser = (e.target as HTMLInputElement).value;
    this.listMemberBySearch = this.listMember;
    if (this.searchUser) {
      this.listMemberBySearch = this.listMember.filter((item) =>
        item.fullNameAndEmail
          .toLowerCase()
          .includes(this.searchUser.toLowerCase().trim())
      );
    }
  }
  handleSearchPm(e: Event) {
    this.searchPm = (e.target as HTMLInputElement).value;
    this.listPmBySearch = this.listPm;
    if (this.searchPm) {
      this.listPmBySearch = this.listPm.filter((item) =>
        item.pmEmailAddress
          .toLowerCase()
          .includes(this.searchPm.toLowerCase().trim()) ||
        item.pmFullName
        .toLowerCase()
        .includes(this.searchPm.toLowerCase().trim())
      );
    }
  }
  close(res): void {
    this._dialogRef.close(res);
  }
  projectChange(project: MatSelectChange){
    if (this.data.action != ActionDialog.EDIT) this.getAllUser(project.value);
    if (this.data.action != ActionDialog.EDIT) this.getAllPm(project.value);
  }
  memberChange(member: MatSelectChange){
    this.temUserId = member.value;
  }
  pmChange(pm: MatSelectChange){
    this.temPmId = pm.value;
  }
}
