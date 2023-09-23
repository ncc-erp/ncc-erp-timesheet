import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { DomSanitizer } from '@angular/platform-browser';
import { TimesheetService } from '@app/service/api/timesheet.service';
import { AppComponentBase } from '@shared/app-component-base';
import { TimeSheetWarningDto } from '../timesheet.component';

@Component({
  selector: 'app-timesheet-warning',
  templateUrl: './timesheet-warning.component.html',
  styleUrls: ['./timesheet-warning.component.css']
})
export class TimesheetWarningComponent extends AppComponentBase implements OnInit {
  approveTimesheetIds;
  listTimeSheetWarning: TimeSheetWarningDto[];
  title;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) 
    public data: any,
    private _dialogRef: MatDialogRef<TimesheetWarningComponent>,
    private timesheetService: TimesheetService,
    private domSanitizer: DomSanitizer,
  ) {
    super(injector);
    this.listTimeSheetWarning = data.item;
    this.approveTimesheetIds =  data.approveTimesheetIds;
    let countApprove = this.approveTimesheetIds.length;
    let countWarning = this.listTimeSheetWarning.length;
    var titleApprove = countApprove == 1 ? `${countApprove} timesheet` : `${countApprove} timesheets`
    var titleWarning = countWarning == 1 ? `${countWarning} timesheet` : `${countWarning} timesheets`
    this.title = `You approve ${titleApprove} but have ${titleWarning} warning`;
  }
  ngOnInit() {
  }

  close(result: any): void {
    this._dialogRef.close(result);
  }

  approveTimesheet() {
    this.timesheetService.approveTimesheet(this.approveTimesheetIds).subscribe((res: any) => {
      if (res) {
        let successMessage = res.result.successCount > 0 ? `<font color='#029720'>${res.result.success} <br /> </font>` : ''
        let failMessage = res.result.failedCount > 0 ? `<font color='#e70d0d'>${res.result.fail} <br /> </font>` : ''
        abp.message.info(successMessage + failMessage + res.result.lockDate, "APPROVED", true)
        this.close(true);
      } else {
        this.notify.warn(this.l(`Approve failed`));
      }
    });
  }

  convertNumberOfStringStatus(status: number) {
    let html = "";
    switch (status) {
      case 0: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-warning'>Draft</span>";
        break;
      }
      case 1: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-info'>Pending</span>";
        break;
      }
      case 2: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-success'>Approved</span>";
        break;
      }
      case 3: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-default'>Rejected</span>";
        break;
      }
    }
    return this.domSanitizer.bypassSecurityTrustHtml(html);
  }

  getChecked(myTimesheetId: number){
    if(this.approveTimesheetIds.includes(myTimesheetId)){
      return true;
    }
    return false;
  }

}
