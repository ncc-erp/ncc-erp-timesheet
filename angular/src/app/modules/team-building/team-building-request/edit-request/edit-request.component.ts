import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { TeamBuildingPMService } from '@app/service/api/team-building-pm.service';
import { EditRequestDto, InputGetAllDetailByRequestIdDto, InputGetUserOtherProjectDto, RequestDto, SelectTeamBuildingDetailDto, SelectUserOtherProjectDto, StatusTeamBuildingDetailEnum } from '../../const/const';
import { TeamBuildingRequestService } from '@app/service/api/team-building-request.service';
import * as _ from 'lodash';
import { CreateEditRequestComponentBase } from '../../CreateEditRequest/CreateEditRequestComponentBase';

@Component({
  selector: 'app-edit-request',
  templateUrl: './edit-request.component.html',
})
export class EditRequestComponent extends CreateEditRequestComponentBase implements OnInit {
   public selectedProjectId: number = null;
   public branchId: number = null;
   public usersAdding: number[] = [];
   public listSelectedItemUserOtherProjectId: number[] = [];

  constructor(
    injector: Injector,
    public teamBuildingPMService: TeamBuildingPMService,
    private teamBuildingRequestService: TeamBuildingRequestService,
    public dialogRef: MatDialogRef<EditRequestComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector, teamBuildingPMService)
   }

  ngOnInit() {
    this.getAllRequest();
    this.getAllProject();
    this.getListBranch();
    this.fileReader.onloadend = () => {
      var base64data = this.fileReader.result;
      this.fileUploadList.push(base64data);
    };
  }

  // Hàm Get Data
  getAllRequest(): void {
    this.isLoading = true;
    const input = {
      teamBuildingHistoryId: this.data,
      projectId: this.selectedProjectId,
      branchId: this.branchId,
      month: this.month,
    } as InputGetAllDetailByRequestIdDto;

    this.teamBuildingRequestService
      .getAllDetailByHistoryId(input)
      .subscribe(
        (res) => {
          const requestAdding = this.requestAdding.filter(
            (item) =>
              (!this.selectedProjectId || item.projectId === this.selectedProjectId) &&
              (!this.month || +(item.applyMonth.split("-")[1].split("T")[0]) === this.month) &&
              (!this.branchId || item.branchId === this.branchId)
          );
          this.requestInfo = [...res.result.teamBuildingDetailDtos
            .map(item => {
              item.selected = !(item.status - StatusTeamBuildingDetailEnum.Requested);
              return item
            }), ...requestAdding ];
          this.originalList = [...res.result.teamBuildingDetailDtos, ...requestAdding ];
          this.isFirst && this.onFirstTime();
          this.lastRemainMoney = res.result.lastRemainMoney;
          this.isLoading = false;
          this.getTotalMoney();
        },
        () => (this.isLoading = false)
      );
  }

  onFirstTime(){
    this.listSelectedItem = this.requestInfo.filter(item => this.getIsSelected(item));
    this.usersAdding = this.requestInfo.filter(item => item.isWarning).map(s => s.employeeId);
    this.isFirst = false;
  }

  getRequestMoneyInfoUser() {
    let requestInfoByUser: RequestDto[] = [];
    this.teamBuildingRequestService
      .getRequestMoneyInfoUserEdit(this.listSelectedItemUserOtherProjectId, this.data)
      .subscribe((res) => {
        requestInfoByUser = res.result.map((t) => {
          return {
            ...t,
            selected: true,
            isNotInProject: true,
          };
        });
        this.requestAdding = [...this.requestAdding,...requestInfoByUser];
        this.listSelectedItem = [...this.listSelectedItem, ...requestInfoByUser.map(item => {
          return {
            id : item.id,
            money : item.money,
            status : item.status,
            selected: true
          } as SelectTeamBuildingDetailDto
        })];

        this.listSelectedItemUserOtherProjectId = [];
        this.listSelectedItemUserOtherProject = [];

        this.getAllRequest();
      });
  }

  handleSelectRequestInfoItem(index, $event) {
      this.requestInfo[index].selected = $event.checked;

      const item = this.listSelectedItem.find(
        (item) => item.id == this.requestInfo[index].id
      );
      if (item) {
        item.selected = $event.checked;
      } else {
        this.listSelectedItem.push({
          selected: $event.checked,
          id: this.requestInfo[index].id,
          money: this.requestInfo[index].money,
          status: this.requestInfo[index].status
        });
      }

    this.updateAllComplete();
    this.checkSelectedCheckbox();
  }

  onChangeUserIdSelected(event) {
    this.listSelectedItemUserOtherProject = event;
    this.getRequestMoneyInfoUser();
  }

  updateAllComplete() {
    this.allComplete =
      this.requestInfo &&
      this.requestInfo.every((t) => this.getIsSelected(t));
    this.getTotalMoney();
  }

  onSaveAndClose() {
    this.saving = true;
    let listIds = this.listSelectedItem.filter((x) => x.selected).map((x) => x.id);

    const request: EditRequestDto = {
      id: this.data,
      listDetailId: listIds,
    };
    this.requestAdding = [];

    this.teamBuildingRequestService.editRequest(request).subscribe(
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

  getTotalMoney() {
    this.totalMoney = 0;
    this.listSelectedItem.forEach((x) => {
      if (this.getIsSelected(x)) {
        this.totalMoney += x.money;
      }
    });
    return this.totalMoney;
  }

  getSelectedUserOtherProject(value: SelectUserOtherProjectDto[]){
    this.listSelectedItemUserOtherProjectId = value.map( item => item.id);
    this.getRequestMoneyInfoUser();
  }

  closePopup(value: boolean) {
    this.isShowBtnAddUser = value;
  }

  close(d) {
    this.dialogRef.close(d);
  }
}
