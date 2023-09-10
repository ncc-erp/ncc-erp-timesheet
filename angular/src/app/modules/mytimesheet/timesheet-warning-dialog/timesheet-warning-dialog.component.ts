import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { MyTimeSheetDto } from '@app/service/api/model/common-DTO';
import { WarningMyTimesheetDto } from '@app/service/api/model/mytimesheet-dto';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { AppComponentBase } from '@shared/app-component-base';
import { TypeAction } from '../const';
import { MyTimeSheetsComponent } from '../mytimesheets.component';


@Component({
  selector: 'app-timesheet-warning-dialog',
  templateUrl: './timesheet-warning-dialog.component.html',
  styleUrls: ['./timesheet-warning-dialog.component.css']
})
export class TimesheetWarningDialogComponent extends AppComponentBase implements OnInit {
  warningMyTimesheet = {} as WarningMyTimesheetDto;
  myTimesheet = {} as MyTimeSheetDto;
  typeAction: TypeAction;
  isLoading: boolean = false;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) 
    public data: any,
    private _dialogRef: MatDialogRef<MyTimeSheetsComponent>,
    private timesheetItemService: MyTimesheetService,
  ) {
    super(injector);
    this.warningMyTimesheet = data.warningMyTimesheet;
    this.myTimesheet = data.myTimesheet;
    this.typeAction = data.typeAction;
  }
  ngOnInit() {
  }

  handleSave(): void {
    this.isLoading = true;
    if(this.typeAction==TypeAction.SAVE){
      this.doSave();
      return;
    }
    this.doSaveAndReset();
  }

  doSave(): void {
    if (this.myTimesheet.id == null) {
      this.timesheetItemService.create(this.myTimesheet).subscribe((result) => {
        this.notify.success(this.l('Create timesheet Item successfully'));
        this._dialogRef.close(result);
      },
        () => this.isLoading = false);
    } else {
      this.timesheetItemService.update(this.myTimesheet).subscribe((result) => {
        this.isLoading = false;
        this.notify.success(this.l('Edit timesheet Item successfully'));
        this._dialogRef.close(result);
      }, (error => this.isLoading = false)
      );
    }
  }

  doSaveAndReset(){
    if (this.myTimesheet.status == 3) {
      this.isLoading = true;
      this.timesheetItemService.saveAndReset(this.myTimesheet).subscribe((result) => {
        this.isLoading = false;
        this.notify.success(this.l('Save and Reset timesheetItem successfully'));
        this._dialogRef.close(result);
      }, (err) => { this.isLoading = false; });
    }
  }

}
