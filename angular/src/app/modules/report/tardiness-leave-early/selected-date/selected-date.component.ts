import { Component, Inject, Injector, OnInit, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { TimekeepingSignalRService } from '@app/service/api/timekeeping-signalR.service';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import * as moment from 'moment';
import { SubscriptionLike } from 'rxjs';

@Component({
  selector: 'app-selected-date',
  templateUrl: './selected-date.component.html',
  styleUrls: ['./selected-date.component.css']
})
export class SelectedDateComponent implements OnInit, OnDestroy {
  
  dateValue: any;
  isSaving: boolean;
  resultMessage:string;

  readonly TimekeepingApiType = APP_CONSTANT.TimekeepingApiType;

  public subscriptionsProcessingDate: SubscriptionLike = null;

  constructor(public dialogref: MatDialogRef<SelectedDateComponent>,
     private service: TimekeepingService,
     private timekeepSignalRService: TimekeepingSignalRService,
    @Inject(MAT_DIALOG_DATA) public data: {useSignalr: boolean, apiType?: string},
     ) {
  }

  ngOnInit() {
    if(this.data.useSignalr) {
      this.subscriptionsProcessingDate = this.timekeepSignalRService.timekeepingProcess.asObservable()
      .subscribe((response) => {
        if(response.event === "requestsuccess") {
          this.isSaving = false;
        }
      });
    }
  }

  ngOnDestroy() {
    if(this.subscriptionsProcessingDate !== null) {
      this.subscriptionsProcessingDate.unsubscribe();
    }
  }

  onDateValueChange(){
    this.resultMessage = '' ;
  }
  saveData() {
    let confirmMessage = '';
    
    switch(this.data.apiType) {
      case APP_CONSTANT.TimekeepingApiType.Snapshot:
        confirmMessage = `<p>Snapshot punishment data for day ${moment(this.dateValue).format("DD/MM/YYYY")}?` + `</p>`;
        break;
      case APP_CONSTANT.TimekeepingApiType.Add:
      default:
        confirmMessage = `<p>Click Submit button will remove all current data on ${moment(this.dateValue).format("DD/MM/YYYY")} and collect data again</p>`;
        break;
    }

    abp.message.confirm(
      confirmMessage,
      "",
      (result: boolean) => {
        if (result) {
          this.isSaving=true;
          const date = moment(this.dateValue).format("YYYY-MM-DD");

          if(!this.data.useSignalr || (this.data.apiType && this.data.apiType !== APP_CONSTANT.TimekeepingApiType.Add)) {
            let apiCall;

            switch(this.data.apiType) {
              case APP_CONSTANT.TimekeepingApiType.Snapshot:
                apiCall = this.service.getSnapshotTimekeepingDay(date);
                break;
              case APP_CONSTANT.TimekeepingApiType.Add:
              default:
                apiCall = this.service.getAddTimeByDay(date);
                break;
            }
            
            apiCall.subscribe(res => {
              this.isSaving=false
              this.resultMessage = `<font color='green'>Successful on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`
            },
            (error)=>{
              this.resultMessage= `<font color='red'>failed on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`
              this.isSaving=false
            })
          } else {
            this.isSaving = true;

            this.timekeepSignalRService.invokeSyncData(date)
              .catch((error) => {
                this.resultMessage= `<font color='red'>failed on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`
                this.isSaving=false
              });

          }
        }
      },
    true);
  }

  close() {
    this.dialogref.close();
  }
}
