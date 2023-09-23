import { Component, Injector, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { RegisterWorkingTimeComponent } from './register-working-time/register-working-time.component';
import * as moment from 'moment';
import { MyWorkingTimeService } from '@app/service/api/my-working-time.service';
import { APP_CONFIG } from '@app/constant/api-config.constant';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-my-working-time',
  templateUrl: './my-working-time.component.html',
  styleUrls: ['./my-working-time.component.css']
})
export class MyWorkingTimeComponent extends AppComponentBase implements OnInit {
  VIEW_MY_WORKING_TIME = PERMISSIONS_CONSTANT.ViewMyWorkingTime;
  REGISTRATION_MY_WORKING_TIME = PERMISSIONS_CONSTANT.RegistrationWorkingTime;
  EDIT_MY_WORKING_TIME = PERMISSIONS_CONSTANT.EditMyWorkingTime;
  DELETE_MY_WORKING_TIME = PERMISSIONS_CONSTANT.DeleteMyWorkingTime;
  
  listStatus = [
    {value: 0, name: 'All'},
    {value: 1, name: 'Pending'},
    {value: 2, name: 'Approved'},
    {value: 3, name: 'Rejected'}
  ]
  registerHistory:RegisterInforDto[] = [];
  currentWorkingTime = {
    morningStartTime: '',
    morningEndTime: '',
    morningWorkingTime: '',
    afternoonStartTime: '',
    afternoonEndTime: '',
    afternoonWorkingTime: ''
  };
  
  constructor(
    public dialog: MatDialog,
    injector: Injector,
    private myWorkingTimeService: MyWorkingTimeService
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getCurrentWorkingTime();
    this.getAllHistoryWorkingTime();
  }
  refresh() {
    this.getCurrentWorkingTime();
    this.getAllHistoryWorkingTime();
  }
  createTime(): void {
    let time = {} as RegisterInforDto;
    time.status = 1;
    time.morningStartTime = this.currentWorkingTime.morningStartTime;
    time.morningEndTime = this.currentWorkingTime.morningEndTime;
    time.morningWorkingTime = this.currentWorkingTime.morningWorkingTime;
    time.afternoonStartTime = this.currentWorkingTime.afternoonStartTime;
    time.afternoonEndTime = this.currentWorkingTime.afternoonEndTime;
    time.afternoonWorkingTime = this.currentWorkingTime.afternoonWorkingTime;
    this.openDialog(time);
  }

  openDialog(infor: RegisterInforDto): void {
    let item = {
      id: infor.id,
      reqestTime: moment().format('L') + " " + moment().format('LT'),
      applyDate: infor.applyDate,
      morningStartTime: infor.morningStartTime,
      morningEndTime: infor.morningEndTime,
      morningWorkingTime: infor.morningWorkingTime,
      afternoonStartTime: infor.afternoonStartTime,
      afternoonEndTime: infor.afternoonEndTime,
      afternoonWorkingTime: infor.afternoonWorkingTime,
      status: infor.status
    } as RegisterInforDto;
    const dialogRef = this.dialog.open(RegisterWorkingTimeComponent, {
      width: '40%',
      data: item
    });

    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        delete result.reqestTime;

        let validTimeObj = this.validRegisterTime(result)
        if(!validTimeObj.success)
        {
          abp.notify.error(validTimeObj.messsage)
          return
        }

        if(!result.id) {
          this.myWorkingTimeService.submitNewWorkingTime(result).subscribe(res => {
            abp.notify.success('Time changed successfully');
            this.getAllHistoryWorkingTime();
          } );
        }
        else {
          this.myWorkingTimeService.editWorkingTime(result).subscribe(res => {
            abp.notify.success('Edit Time successfully');
            this.getAllHistoryWorkingTime();
          } );
        }
      }
    });
  }

  getCurrentWorkingTime() {
    this.myWorkingTimeService.getMyCurrentWorkingTime().subscribe((obj) => {
      this.currentWorkingTime = obj.result;
    })
  }
  getAllHistoryWorkingTime() { 
    this.myWorkingTimeService.getAllMyHistoryWorkingTime().subscribe((result) => {
      this.registerHistory = result.result;
    });
  }

  editRegister(item: RegisterInforDto) {
    this.openDialog(item);
  }
  deleteRegisterInfor(item: RegisterInforDto): void {
    abp.message.confirm(
      "Delete this record ?",
      (result: boolean) => {
        if (result) {
          this.myWorkingTimeService.deleteWorkingTime(item.id).subscribe(res => {
            abp.notify.info('Delete record successfully');
            this.getAllHistoryWorkingTime();
          });
        }
      })
  }

  validRegisterTime(data: RegisterInforDto){
    let result = 
    {
      success: true, 
      messsage: ''
    }

    if(data.morningStartTime > data.morningEndTime)
    {
      result.messsage = "MorningStartTime không thể lớn hơn MorningEndTime!";
    }
    else if(data.morningEndTime > data.afternoonStartTime)
    {
      result.messsage = "MorningEndTime không thể lớn hơn AfternoonStartTime!";
    }
    else if(data.afternoonStartTime > data.afternoonEndTime)
    {
      result.messsage = "AfternoonStartTime không thể lớn hơn AfternoonEndTime!";
    }

    if(result.messsage != '')
      result.success = false;

    return result;
  }
}

export interface RegisterInforDto {
  id?: number,
  reqestTime?: string,
  applyDate?: string,
  morningStartTime: string,
  morningEndTime: string,
  morningWorkingTime?: string,
  afternoonStartTime: string,
  afternoonEndTime: string,
  afternoonWorkingTime?: string,
  status?: number
}