import { Component, Inject, Injector, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { AppComponentBase } from "@shared/app-component-base";
import * as moment from "moment";
import {
  DateAdapter,
  MAT_DATE_FORMATS,
  MAT_DATE_LOCALE,
} from "@angular/material/core";
import {
  MomentDateAdapter,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
} from "@angular/material-moment-adapter";

import { UserService } from "@app/service/api/user.service";
import * as FileSaver from "file-saver";
import { TimeGetDataCheckpointDialog, TimeGetDataCheckpointDto } from "@app/service/api/model/time-checkpoint-dto";

export const MY_FORMATS = {
  parse: {
    dateInput: "DD/MM/YYYY",
  },
  display: {
    dateInput: "DD/MM/YYYY",
  },
};

@Component({
  selector: 'app-export-data-check-point',
  templateUrl: './export-data-check-point.component.html',
  styleUrls: ['./export-data-check-point.component.css'],
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps:  [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },

    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})
export class ExportDataCheckPointComponent extends AppComponentBase implements OnInit {
  public timeCheckpoint = {} as TimeGetDataCheckpointDto;
  public formData = this.fb.group({
    startDate : [moment().startOf("month").toDate(), Validators.required],
    endDate: [moment().endOf("month").toDate(), Validators.required],
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: TimeGetDataCheckpointDialog,
    injector: Injector,
    private _dialogRef: MatDialogRef<ExportDataCheckPointComponent>,
    private fb: FormBuilder,
    public userService: UserService
  ) { 
    super(injector);
    this.timeCheckpoint = this.data.item;
    this.formData.patchValue({
      startDate: moment(this.timeCheckpoint.startDate).toDate(),
      endDate: moment(this.timeCheckpoint.endDate).toDate(),
    });
  }

  ngOnInit() {
  }
  private convertFile(fileData) {
    var buf = new ArrayBuffer(fileData.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != fileData.length; ++i)
      view[i] = fileData.charCodeAt(i) & 0xff;
    return buf;
  }

  public export(){
    const checkEndDate = moment(this.formData.controls["endDate"].value).diff(
      this.formData.controls["startDate"].value,
      "day"
    );
    if(checkEndDate <= 0){
      abp.message.error("start time must be less than end time");
      return;
    }
    this.timeCheckpoint = {...this.formData.value};
    var startDate = moment(this.timeCheckpoint.startDate).format("YYYY/MM/DD");
    var endDate =  moment(this.timeCheckpoint.endDate).format("YYYY/MM/DD");
    this.userService.ExportDataCheckpoint(startDate, endDate)
      .subscribe((rs) => {
        const file = new Blob([this.convertFile(atob(rs.result.base64))], {
          type: "application/vnd.ms-excel;charset=utf-8",
        });
        FileSaver.saveAs(file, "ExportDataCheckPoint.xlsx");
      });
  }
  close(res): void {
    this._dialogRef.close(res);
  }
}
