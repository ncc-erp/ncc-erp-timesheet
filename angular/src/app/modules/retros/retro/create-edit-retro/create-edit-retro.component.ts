import { Component, Inject, Injector, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import {
  RetroActionDialogData,
  RetroDto,
} from "@app/service/api/model/retro-dto";
import { RetroService } from "@app/service/api/retro.service";
import { AppComponentBase } from "@shared/app-component-base";
import { ActionDialog } from "@shared/AppEnums";
import * as moment from "moment";
import { status } from "../retro.component";
import {
  DateAdapter,
  MAT_DATE_FORMATS,
  MAT_DATE_LOCALE,
} from "@angular/material/core";
import {
  MomentDateAdapter,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
} from "@angular/material-moment-adapter";

export const MY_FORMATS = {
  parse: {
    dateInput: "DD/MM/YYYY",
  },
  display: {
    dateInput: "DD/MM/YYYY",
    monthYearLabel: "MMM YYYY",
    dateA11yLabel: "LLL",
    monthYearA11yLabel: "MMMM YYYY",
  },
};

@Component({
  selector: "app-create-edit-retro",
  styleUrls: ["./create-edit-retro.component.css"],
  templateUrl: "./create-edit-retro.component.html",
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },

    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})
export class CreateEditRetroComponent
  extends AppComponentBase
  implements OnInit
{
  public retro = {} as RetroDto;
  public title: string;
  public isSaving: boolean = false;
  public action: ActionDialog;

  public formData = this.fb.group({
    name: [
      `Retro tháng ${moment().format("MM")} - ${moment().format("YYYY")}`,
      Validators.required,
    ],
    startDate: [moment().startOf("month").toDate(), Validators.required],
    endDate: [moment().endOf("month").toDate(), Validators.required],
    deadline: [moment().endOf("month").toDate(), Validators.required],
    status: [0, Validators.required],
  });

  public listStatus: status[] = [{ value: 2, title: "Draft" }];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: RetroActionDialogData,
    injector: Injector,
    private _dialogRef: MatDialogRef<CreateEditRetroComponent>,
    private fb: FormBuilder,
    public retroService: RetroService
  ) {
    super(injector);
    this.retro = this.data.item;
    this.action = this.data.action;
    if (this.action === ActionDialog.EDIT) {
      this.formData.patchValue({
        name: this.retro.name,
        startDate: moment(this.retro.startDate).toDate(),
        endDate: moment(this.retro.endDate).toDate(),
        deadline: moment(this.retro.deadline).toDate(),
        status: this.retro.status,
      });
    }
  }

  ngOnInit(): void {
    this.setTitleDialog();
  }

  private setTitleDialog(): void {
    if (this.action === ActionDialog.CREATE) {
      this.title = "New Retro";
    } else {
      this.title = "Update Retro";
    }
  }

  public onSave() {
    const checkEndDate = moment(this.formData.controls["endDate"].value).diff(
      this.formData.controls["startDate"].value,
      "day"
    );

    const checkDeadline = moment(this.formData.controls["deadline"].value).diff(
      this.formData.controls["startDate"].value,
      "day"
    );
    if (checkEndDate <= 0) {
      abp.message.error("Start time must be less than end time");
      return;
    }
    if (checkDeadline <= 0) {
      abp.message.error("Start time must be less than deadline");
      return;
    }
    this.retro = { ...this.formData.value, id: this.retro.id };
    if (this.action === ActionDialog.CREATE) {
      this.doCreate();
    } else {
      this.doUpdate();
    }
  }

  public doCreate() {
    this.isSaving = true;
    this.retroService.create(this.retro).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Create Retro successfully"));
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
    this.retroService.update(this.retro).subscribe(
      (res) => {
        if (res.success) {
          this.notify.success(this.l("Update Retro successfully"));
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
