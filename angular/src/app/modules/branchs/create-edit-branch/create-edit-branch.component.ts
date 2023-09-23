import { AppComponentBase } from '@shared/app-component-base';
import { BranchService } from '../../../service/api/branch.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import * as _ from 'lodash';
import { BranchCreateEditDto, BranchDto } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
@Component({
  selector: 'app-create-edit-branch',
  templateUrl: './create-edit-branch.component.html',
  styleUrls: ['./create-edit-branch.component.css']
})
export class CreateEditBranchComponent extends AppComponentBase implements OnInit {
  branch = {} as BranchCreateEditDto;
  title: string;
  active: boolean = true;
  respone: number;
  saving: boolean = false;
  isSaving: boolean = false;
  closePopup: number = 1;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: BranchCreateEditDto,
    injector: Injector,
    private branchService: BranchService,
    private _dialogRef: MatDialogRef<CreateEditBranchComponent>,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.branch = this.data;
    this.title = this.branch.id != null ? 'Edit Branch' : 'New Branch';
  }

  onChangeTime(event, name, workingTime) {
    if (workingTime == 'morning') {
      if (name == 'morningStartAt') {
        this.branch.morningWorking = this.calculationTime(event, this.branch.morningEndAt);
      }
      else {
        this.branch.morningWorking = this.calculationTime(this.branch.morningStartAt, event);
      }
      this.branch.morningWorking = Number(this.convertTimeToNumber(this.branch.morningWorking).toFixed(2).toString())
    }
    else {
      if (name == 'afternoonStartAt') {
        this.branch.afternoonWorking = this.calculationTime(event, this.branch.afternoonEndAt);
      }
      else {
        this.branch.afternoonWorking = this.calculationTime(this.branch.afternoonStartAt, event);
      }
      this.branch.afternoonWorking = Number(this.convertTimeToNumber(this.branch.afternoonWorking).toFixed(2).toString())
    }
  }

  convertTimeToNumber(time) {
    var hoursMinutes = time.split(/[.:]/);
    var hours = parseInt(hoursMinutes[0], 10);
    var minutes = hoursMinutes[1] ? parseInt(hoursMinutes[1], 10) : 0;
    return hours + minutes / 60;
  }

  calculationTime(timeStart: string, timeEnd: string): string {
    let time = moment(timeEnd, 'HH:mm').diff(moment(timeStart, 'HH:mm'), 'minutes');
    var hours = time / 60 | 0, minutes = time % 60 | 0;
    return moment.utc().hours(hours).minutes(minutes).format("HH:mm");
  }

  formatTime(time: string) {
    if (time) {
      if (time.includes(':')) {
        let t = time.split(':');
        if (t[1]) {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(Number.parseInt(t[1])).format("HH:mm");
        } else {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(0).format("HH:mm");
        }
      } else {
        return moment.utc().hours(Number.parseInt(time)).minutes(0).format("HH:mm");
      }
    }
  }

  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]

  public maskHour = {
    guide: false,
    showMask: false,
    mask: [/\d/, /\d/],
  }

  public maskMinute = {
    guide: false,
    showMask: false,
    mask: [/[0-5]/, /[0-9]/]
  };

  save() {
    if (!this.branch.name) {
      abp.message.error("Name is required!")
      return;
    }

    if (!this.branch.displayName) {
      abp.message.error("Display name is required!")
      return;
    }


    this.isSaving = true;
    this.branchService.save(this.branch).subscribe(res => {
      if (res.success) {
        if (this.branch.id == null) {
          this.notify.success(this.l('Create Branch successfully'));
        }
        else {
          this.notify.success(this.l('Update Branch successfully'));
        }
   
        this.respone = this.closePopup;
        this.close(this.respone);
      }
    }, err => {
      this.isSaving = false;
    })
  }

  close(res): void {
    this._dialogRef.close(res);
  }
}
