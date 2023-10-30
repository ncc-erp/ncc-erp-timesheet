import { Component, Injector, OnInit, Inject } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { SelectUserOtherProjectDto } from '@app/modules/team-building/const/const';
import { AppServiceBase } from '@app/service/api/appServiceBase.service';
import { PositionDto } from '@app/service/api/model/position-dto';
import { ProjectByCurrentUserDto } from '@app/service/api/model/project-Dto';
import { RetroDetailCreateEditDto, RetroDetailDialogData, RetroDetailDto } from '@app/service/api/model/retro-detail-dto';
import { PositionService } from '@app/service/api/position.service';
import { RetroDetailService } from '@app/service/api/retro-detail.service';
import { AppComponentBase } from '@shared/app-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import * as _ from 'lodash';

@Component({
  selector: 'app-add-multi-retro-detail',
  templateUrl: './add-multi-retro-detail.component.html',
  styleUrls: ['./add-multi-retro-detail.component.css']
})
export class AddMultiRetroDetailComponent extends AppComponentBase implements OnInit {

  public project: ProjectByCurrentUserDto[] = [];
  public projectFilter: ProjectByCurrentUserDto[] = [];
  public selectedProjectId = 0;
  public projectSearch = "";

  public listBranch: BranchDto[] = [];
  public branchSearch: "";
  public listBranchFilter: BranchDto[];
  public branchId = 0;

  public retroId: number;
  public isLoading: boolean = false;
  public retroResultAdding: RetroDetailDto[] = [];
  public requestInfo: RetroDetailDto[] = [];
  public originalList: RetroDetailDto[] = [];

  public currentSortColumn: string = "transactionDate";
  public sortDirection: number = 0;
  public iconSort: string = "";
  public listPosition: PositionDto[] = [];
  public isShowBtnAddUser = false;
  public listSelectedItemUserOtherProjectId: number [] = [];
  public listSelectedItemUserOtherProject: SelectUserOtherProjectDto[] = [];
  public saving: boolean;

  public listAddRetroResult: RetroDetailCreateEditDto [] = [];
  public retroResultFormGroup: FormGroup = new FormGroup({});

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: RetroDetailDialogData[],
    private dialogRef: MatDialogRef<AddMultiRetroDetailComponent>,
    injector: Injector,
    public appServiceBase: AppServiceBase,
    private formbuilder: FormBuilder,
    public retroResultService: RetroDetailService,
    private positionService: PositionService,
    private route: ActivatedRoute,
  ) {
    super(injector);
    this.retroId = route.snapshot.queryParams.retroId;
  }

  ngOnInit() {
    this.getAllProject();
    this.getAllBranch();
    this.getAllRequest();
    this.positionService.GetAllPosition().subscribe((value) => {
      this.listPosition = value.result.items;
    });
    this.initRetroResultFormArray();
  }

  initRetroResultFormArray() {
    this.retroResultFormGroup = this.formbuilder.group({
      retroResultFormArray: this.formbuilder.array([])
    })
  }

  createRetroResultForm(requestInfo: RetroDetailDto[]) {
    requestInfo.forEach((item) => {
      const retroFormGroup = this.formbuilder.group({
        userId: new FormControl(item.userId),
        projectId: new FormControl(item.projectId),
        positionId: new FormControl(item.positionId),
        retroId: new FormControl(this.retroId),
        pmId: new FormControl(item.pmId),
        point: new FormControl(item.point),
        note: new FormControl(item.note),
        branchId: new FormControl(item.branchId),
        id: new FormControl(item.id),
        projectName: new FormControl(item.projectName),
        userLevel: new FormControl(item.level),
        userName: new FormControl(item.userName),
        userType: new FormControl(item.userType),
        fullName: new FormControl(item.fullName),
        emailAddress: new FormControl(item.emailAddress),
        branchColor: new FormControl(item.branchColor),
        branchName: new FormControl(item.branchName)
      });
      (this.retroResultFormGroup.controls["retroResultFormArray"] as FormArray).push(retroFormGroup);
    })
  }

  deleteRetroResultForm(index : number) {
    const formArray = this.retroResultFormGroup.get("retroResultFormArray") as FormArray;
    formArray.removeAt(index);
  }

  getAllRequest(): void {
    this.isLoading = true;
    this.retroResultService
      .getAllRetroResultByPM(this.retroId, this.selectedProjectId, this.branchId)
      .subscribe(
        (res) => {
          const retroResultAdding = this.retroResultAdding.filter(
            (item) =>
              (!this.selectedProjectId || item.projectId === this.selectedProjectId) &&
              (!this.branchId || item.branchId === this.branchId)
          );
           this.requestInfo = this.retroResultAdding = [...res.result, ...retroResultAdding];
           this.createRetroResultForm(this.requestInfo);
           this.originalList = [...res.result, ...retroResultAdding];
          this.isLoading = false;
        },
        () => (this.isLoading = false)
      );
  }

  onSaveAndClose() {
    this.saving = true;
    let formArray = this.retroResultFormGroup.controls["retroResultFormArray"] as FormArray;
    let retroDtoList: RetroDetailCreateEditDto[] = []; // Khởi tạo mảng trống

    for (let item of formArray.controls) {
      if (item instanceof FormGroup) {
        let retroDto: RetroDetailCreateEditDto = {
          userId: item.controls["userId"].value,
          projectId: item.controls["projectId"].value,
          positionId: item.controls["positionId"].value,
          retroId: item.controls["retroId"].value,
          pmId: item.controls["pmId"].value,
          point: item.controls["point"].value,
          note: item.controls["note"].value,
          branchId: item.controls["branchId"].value,
          id: item.controls["id"].value,
          projectName: item.controls["projectName"].value,
          userLevel: item.controls["userLevel"].value,
          userName: item.controls["userName"].value,
          userType: item.controls["userType"].value,
        };

        retroDtoList.push(retroDto);
      }
    }

    this.retroResultAdding = [];

    this.retroResultService.addMultiUserRetroResult(retroDtoList).subscribe(
      (response) => {
        if (response) {
          abp.notify.success("PM send request successful");
          this.saving = false;
          this.dialogRef.close(true);
        }
      },
      () => (this.saving = false)
    );
  }


  getRetroResultInfoUser() {
    let retroResultInfoByUser: RetroDetailDto[] = [];
    this.retroResultService
    .getRetroResultInfoUser(this.listSelectedItemUserOtherProjectId, this.retroId)
    .subscribe((res) => {
      retroResultInfoByUser = res.result.map((t) => {
        return {
          ...t,
          isNotInProject: true,
        };
      });
      this.retroResultAdding = [...this.retroResultAdding, ...retroResultInfoByUser];
      this.listSelectedItemUserOtherProjectId = [];
      this.listSelectedItemUserOtherProject = [];

      this.getAllRequest();
    })
  }

  getAllProject() {
    this.appServiceBase.getAllProjectByCurrentUser().subscribe((data) => {
      this.project = this.projectFilter = data.result;
    });
  }

  getAllBranch() {
    this.appServiceBase.getAllCurrentBranch().subscribe((res) => {
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
        case "emailAddress":
          this.requestInfo.sort((a, b) => {
            return this.sortDirection == 1
              ? b.emailAddress.localeCompare(a.emailAddress)
              : a.emailAddress.localeCompare(b.emailAddress);
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
      }
    } else {
      this.iconSort = "fas fa-sort";
      this.requestInfo = [...this.originalList];
    }
  }

  onChangeUserIdSelected(event) {
    this.listSelectedItemUserOtherProject = event;
    this.getRetroResultInfoUser();
  }

  close(d) {
    this.dialogRef.close(d);
  }

  getSelectedUserOtherProjectRetro(value: SelectUserOtherProjectDto[]){
    this.listSelectedItemUserOtherProjectId = value.map( item => item.id);
    this.getRetroResultInfoUser();
  }

  toggleShowAddUserDialog(){
    this.isShowBtnAddUser = !this.isShowBtnAddUser;
  }

  closePopup(value: boolean) {
    this.isShowBtnAddUser = value;
  }
}
