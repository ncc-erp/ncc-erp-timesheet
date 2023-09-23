import { Component, Inject, Injector, OnInit} from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { EditMoneyTeamBuildingDetailDialogData, EditMoneyTeamBuildingDetailDto } from '../../const/const';
import { ActionDialog } from '@shared/AppEnums';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { TeamBuildingHRService } from '@app/service/api/team-building-hr.service';

@Component({
  selector: 'app-edit-money',
  templateUrl: './edit-money.component.html',
})
export class EditMoneyComponent
extends AppComponentBase
implements OnInit {
  teamBuildingDetail = {} as EditMoneyTeamBuildingDetailDto;
  isSaving: boolean = false;
  action: ActionDialog;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: EditMoneyTeamBuildingDetailDialogData,
    injector: Injector,
    private _teamBuildingDetailService: TeamBuildingHRService,
    private _dialogRef: MatDialogRef<EditMoneyComponent>
  ) {
    super(injector);
    this.teamBuildingDetail = this.data.item;
    this.action = this.data.action;
   }

  ngOnInit() {
  }

  doUpdate() {
    this.isSaving = true;
    this._teamBuildingDetailService.update(this.teamBuildingDetail).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Update Money successfully"));
          this.close(true);
        }
      },
      (err) => {
        this.isSaving = false;
      }
    );
  }

  close(refresh: boolean): void {
    this._dialogRef.close(refresh);
  }
}
