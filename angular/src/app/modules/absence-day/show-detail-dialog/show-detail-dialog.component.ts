import { AppComponentBase } from 'shared/app-component-base';
import { AbsenceDayService } from '@app/service/api/absence-day.service';
import { Component, OnInit, Injector } from '@angular/core';
import { Inject } from "@node_modules/@angular/core";
import { MAT_DIALOG_DATA } from "@node_modules/@angular/material";
import { AbsenceRequestDetailDto, LeaveDayDTO } from '@app/service/api/model/absence-day-dto';
import { moment } from 'ngx-bootstrap/chronos/test/chain';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-show-detail-dialog',
  templateUrl: './show-detail-dialog.component.html',
  styleUrls: ['./show-detail-dialog.component.css']
})
export class ShowDetailDialogComponent extends AppComponentBase implements OnInit {

  label: LeaveDayDTO;
  currentDate :any;
  isLoading :boolean = false;
  constructor(@Inject(MAT_DIALOG_DATA) public data: any ,
  private absenceDayService : AbsenceDayService,
  private dialogRef: MatDialogRef<ShowDetailDialogComponent>,
  private injector : Injector){super(injector)}

  ngOnInit() {
    this.label = new LeaveDayDTO;
    this.currentDate = moment(new Date()).format('YYYY-MM-DD');
    console.log(this.data)
    if(this.data.type === 0){
      let reason = this.data.reason ? this.data.reason : "No reason";
      this.label.titleLabel = "Details Of Absence Days";
      this.label.statusLabel = "Status: ";
      this.label.absenceTypeLabel = "Type: " + this.data.dayOffName;
      this.label.reasonLabel = "Reason: " + reason;
      this.label.leaveDayLabel = "Absence Days: ";
    } else if(this.data.type === 1) {
      let reason = this.data.reason ? this.data.reason : "Nothing";
      this.label.titleLabel = "Details Of Onsite Days";
      this.label.statusLabel = "Status: ";
      this.label.absenceTypeLabel = "";
      this.label.reasonLabel = "Note: " + reason;
      this.label.leaveDayLabel = "Onsite Days: ";
    } else {
      let reason = this.data.reason ? this.data.reason : "Nothing";
      this.label.titleLabel = "Details Of Remote Days";
      this.label.statusLabel = "Status: ";
      this.label.absenceTypeLabel = "";
      this.label.reasonLabel = "Note: " + reason;
      this.label.leaveDayLabel = "Remote Days: ";
    }
  }

  getLeaveDayByDateType(data: AbsenceRequestDetailDto) {
    if (data.dateType === 1)
      return "Full day";

    if (data.dateType === 2)
      return "Morning";

    if (data.dateType === 3)
      return "Afternoon";

    if (data.dateType === 4)
      return "";
  }
  cancelRequest(id){
    this.isLoading = true;
    this.absenceDayService.cancelAbsenceDayRequest(id).subscribe((res)=>{
      if(res){
        abp.notify.success("Cancel Request Successfully!");
        this.dialogRef.close(id);
      }
    },()=> this.isLoading = false)
  }
}
