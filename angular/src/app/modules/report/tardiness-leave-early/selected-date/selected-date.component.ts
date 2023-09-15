import { Component, Inject, Injector, OnInit, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { TimekeepingSignalRService } from '@app/service/api/timekeeping-signalR.service';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import * as moment from 'moment';
import { SubscriptionLike } from 'rxjs';

@Component({
  selector: 'app-selected-date',
  templateUrl: './selected-date.component.html',
  styleUrls: ['./selected-date.component.css']
})
export class SelectedDateComponent implements OnInit {
  
  dateValue: any;
  isSaving: boolean;
  resultMessage:string;

  public subscriptionsProcessingDate: SubscriptionLike = null;

  constructor(public dialogref: MatDialogRef<SelectedDateComponent>,
     private service: TimekeepingService,
     private timekeepSignalRService: TimekeepingSignalRService,
    @Inject(MAT_DIALOG_DATA) public data: {useSignalr: boolean},
     ) {
  }

  ngOnInit() {
    if(!this.data.useSignalr) {
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
    abp.message.confirm(
     `<p>Click Submit button will remove all current data on ${ moment(this.dateValue).format("DD/MM/YYYY")} and collect data again</p>`,
      "",
      (result: boolean) => {
        if (result) {
          this.isSaving=true;
          const date = moment(this.dateValue).format("YYYY-MM-DD");
          if(!this.data.useSignalr) {
            this.service.getAddTimeByDay(date).subscribe(res => {
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
