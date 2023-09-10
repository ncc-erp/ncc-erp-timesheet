import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from "@app/service/api/absence-request.service";
import { Injector } from "@node_modules/@angular/core";
import { MatCheckboxChange } from "@node_modules/@angular/material";
import { AppComponentBase } from "@shared/app-component-base";
import { AppConsts } from '@shared/AppConsts';

@Component({
  selector: 'app-approve-absence-request',
  templateUrl: './approve-absence-request.component.html',
  styleUrls: ['./approve-absence-request.component.css']
})
export class ApproveAbsenceRequestComponent extends AppComponentBase implements OnInit {
  APPROVAL_LEAVE_DAY = PERMISSIONS_CONSTANT.ApprovalLeaveDay;

  isLoading: boolean;
  viewByState;
  fromDate: any;
  toDate: any;
  typeDate: any;
  AbsenceRequestState = [
    { value: 0, name: 'All', count: 0 },
    { value: 1, name: 'Pending', count: 0 },
    { value: 2, name: 'Approved', count: 0 },
    { value: 3, name: 'Rejected', count: 0 }
  ];
  userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Internship' },
    { value: 2, label: 'Collaborator' }
  ];
  absenceRequests: AbsenceRequest[];
  backupAbsenceRequests: AbsenceRequest[];
  selectedReqIds: Set<number>;
  constructor(injector: Injector, private absenceRequestService: AbsenceRequestService, private changeDetector: ChangeDetectorRef) {
    super(injector);
  }

  ngOnInit() {
    this.viewByState = this.AbsenceRequestState[1].value;
    this.selectedReqIds = new Set<number>();
  }

  ngAfterViewChecked() {
    this.changeDetector.detectChanges();
  }

  refresh() {
    this.isLoading = true
    this.absenceRequestService.getAll(this.fromDate, this.toDate, this.AbsenceRequestState[0].value)
      .subscribe(resp => {
        this.backupAbsenceRequests = resp.result as AbsenceRequest[];
        this.absenceRequests = this.backupAbsenceRequests.filter(a => (this.viewByState === this.AbsenceRequestState[0].value) ? true : a.status === this.viewByState);
        this.AbsenceRequestState[0].count = this.backupAbsenceRequests.length;
        this.AbsenceRequestState[1].count = this.backupAbsenceRequests.filter(a => a.status === this.AbsenceRequestState[1].value).length;
        this.AbsenceRequestState[2].count = this.backupAbsenceRequests.filter(a => a.status === this.AbsenceRequestState[2].value).length;
        this.AbsenceRequestState[3].count = this.backupAbsenceRequests.filter(a => a.status === this.AbsenceRequestState[3].value).length;
      });
    this.selectedReqIds.clear();
    this.isLoading = false

  }

  btnApproveClicked() {
    if (this.selectedReqIds.size <= 0) {
      abp.message.error("Bạn chưa chọn ngày nào !")
    } else {
      abp.message.confirm("Xác nhận Approve ?",
        (result: boolean) => {
          if (result) {
            this.absenceRequestService.approveAbsenceRequest(Array.from(this.selectedReqIds)).subscribe(resp => {
              if (resp.success == true) {
                abp.message.success("Off day request(s) has approved!");
                this.refresh();
              }
            });
          }  
        }  
      )
    }
  }
  btnRejectClicked() {
    if (this.selectedReqIds.size <= 0) {
      abp.message.error("Bạn chưa chọn ngày nào !")
    } else {
      abp.message.confirm("Xác nhận Reject ?",
        (result: boolean) => {
          if (result) {
            this.absenceRequestService.rejectAbsenceRequest(Array.from(this.selectedReqIds)).subscribe((resp) => {
              if (resp.success == true) {
                abp.message.success("Off day request(s) has rejected!");
                this.refresh();
              }
            });
          }   
        }
      )
    }
  }
  handleDateSelectorChange(date) {
    const { fromDate, toDate } = date;
    this.setFromAndToDate(fromDate, toDate);
    this.refresh()
  }

  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }

  changeViewBy() {
    if (this.viewByState === this.AbsenceRequestState[0].value) {
      this.absenceRequests = this.backupAbsenceRequests;
    } else {
      this.absenceRequests = this.backupAbsenceRequests.filter(a => a.status === this.viewByState);
    }
    this.selectedReqIds.clear();
  }
  checkboxRequest($event: MatCheckboxChange, id: number) {
    let isChecked = $event.source.checked;
    if (isChecked) {
      this.selectedReqIds.add(id);
    } else {
      this.selectedReqIds.delete(id);
    }
  }


  getAbsenceTypeName(detail: RequestDetailDto){
    if (detail.dateType == this.APP_CONSTANT.AbsenceType.FullDay){
      return 'Off Full day';
    }

    if (detail.dateType == this.APP_CONSTANT.AbsenceType.Morning){
      return 'Off Morning';
    }

    if (detail.dateType == this.APP_CONSTANT.AbsenceType.Afternoon){
      return 'Off Afternoon';
    }

    if (detail.dateType == this.APP_CONSTANT.AbsenceType.Custom){
      return 'Off ' + detail.hour + ' h';
    }

  }

}

class AbsenceRequest {
  avatarPath: string;
  avatarFullPath: string;
  userId: number;
  fullName: string;
  dayOffName: string;
  status: number;
  reason: string;
  type: number;
  level: number;
  sex: number;
  branch: number;
  details: {
    dateAt: string;
    dateType: number;
    id: number;
    hour: number;
  }[];
  id: number;
}

class RequestDetailDto{
    dateAt: string;
    dateType: number;
    id: number;
    hour: number;
}
