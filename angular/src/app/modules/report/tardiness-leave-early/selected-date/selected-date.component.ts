import { Component, Inject, Injector, OnInit, Output, EventEmitter } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';

@Component({
  selector: 'app-selected-date',
  templateUrl: './selected-date.component.html',
  styleUrls: ['./selected-date.component.css']
})
export class SelectedDateComponent implements OnInit {
  dateValue: any;
  isSaving: boolean;
  resultMessage:string;
  constructor(public dialogref: MatDialogRef<SelectedDateComponent>, private service: TimekeepingService) {
  }
  ngOnInit() {
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
          this.isSaving=true
          const date = moment(this.dateValue).format("YYYY-MM-DD");
          this.service.getAddTimeByDay(date).subscribe(res => {
            this.isSaving=false
            this.resultMessage = `<font color='green'>Successful on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>` 
          },()=>{
            this.resultMessage= `<font color='red'>failed on ${moment(this.dateValue).format("DD/MM/YYYY")}</font>` 
            this.isSaving=false
          })
        }
      },
    true);
  }
}
