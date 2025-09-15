import { Component, Inject, Injector, OnInit, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { TimekeepingSignalRService } from '@app/service/api/timekeeping-signalR.service';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import * as moment from 'moment';
import { SubscriptionLike } from 'rxjs';
import swal from 'sweetalert';

@Component({
  selector: 'app-selected-date',
  templateUrl: './selected-date.component.html',
  styleUrls: ['./selected-date.component.css']
})
export class SelectedDateComponent implements OnInit, OnDestroy {
  
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
    this.dialogref.close();

    swal({
      title: "Are you sure?",
      content: {
        element: "div",
        attributes: {
          innerHTML: `<p>Click Submit button will remove all current data on ${moment(this.dateValue).format("DD/MM/YYYY")} and collect data again</p>`
        },
      },
      icon: "warning",
      buttons: ["Cancel", "Yes"],
      dangerMode: true
    })
      .then((willDelete) => {
        if (willDelete) {
          this.isSaving = true;
          const date = moment(this.dateValue).format("YYYY-MM-DD");

          swal({
            title: "Processing...",
            text: "Please wait while we process your request.",
            icon: "info",
            buttons: [false],
            closeOnClickOutside: false,
            closeOnEsc: false
          });
          
          if(!this.data.useSignalr) {
            this.service.getAddTimeByDay(date).subscribe(res => {
              this.isSaving = false;
              this.resultMessage = `<font color='green'>Successful on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`;

              (swal as any).close();
              setTimeout(() => {
                swal("Success", `Data processed successfully for ${moment(this.dateValue).format("DD/MM/YYYY")}`, "success");
              }, 100);
            },
            (error)=>{
              this.isSaving = false;
              this.resultMessage = `<font color='red'>failed on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`;

              (swal as any).close();
              setTimeout(() => {
                swal("Error", error.error ? error.error.message : "An error occurred", "error");
              }, 100);
            })
          } else {
            this.timekeepSignalRService.invokeSyncData(date)
              .then(() => {
                setTimeout(() => {
                  if (this.isSaving) {
                    this.isSaving = false;
                    (swal as any).close();
                    setTimeout(() => {
                      swal("Info", "Request sent. Processing may take some time.", "info");
                    }, 100);
                  }
                }, 3000);
              })
              .catch((error) => {
                this.isSaving = false;
                this.resultMessage = `<font color='red'>failed on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>`;

                (swal as any).close();
                setTimeout(() => {
                  swal("Error", error.message || "An error occurred", "error");
                }, 100);
              });
          }
        }
      })
  }

  close() {
    this.dialogref.close();
  }
}
