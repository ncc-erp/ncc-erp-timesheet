import { AppComponentBase } from "@shared/app-component-base";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { Component, OnInit, Injector, Inject } from "@angular/core";
import * as _ from "lodash";
import * as moment from "moment";
import {
  PositionCreateEditDto,
  PositionDialogData,
} from "@app/service/api/model/position-dto";
import { PositionService } from "@app/service/api/position.service";
import { ActionDialog } from "@shared/AppEnums";
@Component({
  selector: "app-create-edit-position",
  templateUrl: "./create-edit-position.component.html",
  styleUrls: ["./create-edit-position.component.css"],
})
export class CreateEditPositionComponent
  extends AppComponentBase
  implements OnInit
{
  position = {} as PositionCreateEditDto;
  title: string;
  isSaving: boolean = false;
  action: ActionDialog;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PositionDialogData,
    injector: Injector,
    private positionService: PositionService,
    private _dialogRef: MatDialogRef<CreateEditPositionComponent>
  ) {
    super(injector);
    this.position = this.data.item;
    this.action = this.data.action;
  }

  ngOnInit() {
    this.setTitleDialog();
  }
  private setTitleDialog(): void {
    if (this.action == ActionDialog.EDIT) {
      this.title = `Update <strong>${this.position.name}</strong> position`;
    } else {
      this.title = "New position";
    }
  }

  onSave() {
    if (!this.position.name) {
      abp.message.error("Name is required!");
      return;
    }

    if (!this.position.shortName) {
      abp.message.error("Short name is required!");
      return;
    }
    this.isSaving = true;
    const tmp = this.position;
    this.position = {
      name: tmp.name.trim(),
      shortName: tmp.shortName.trim(),
      code: tmp.code,
      color: tmp.color,
      id: tmp.id,
    };
    if (this.action == ActionDialog.CREATE) {
      this.doCreate();
      return;
    }
    this.doUpdate();
  }

  doCreate() {
    this.isSaving = true;
    this.positionService.create(this.position).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Create Position successfully"));
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
    this.positionService.update(this.position).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Update Position successfully"));
          this.close(1);
        }
      },
      (err) => {
        this.isSaving = false;
      }
    );
  }

  close(res): void {
    this._dialogRef.close(res);
  }
}
