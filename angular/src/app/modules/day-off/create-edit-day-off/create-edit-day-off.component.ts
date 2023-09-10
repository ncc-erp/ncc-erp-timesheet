import { Component, OnInit, Injector, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { FormControl, Validators } from '@angular/forms';
import { dayOffDTO } from '../day-off.component';
import { DayOffService } from '@app/service/api/day-off.service';
import * as moment from 'moment';

@Component({
  selector: 'app-create-edit-day-off',
  templateUrl: './create-edit-day-off.component.html',
  styleUrls: ['./create-edit-day-off.component.css']
})
export class CreateEditDayOffComponent extends AppComponentBase implements OnInit {

  dateControl = new FormControl("", [Validators.required]);
  title;
  dayOff: dayOffDTO;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private diaLogRef: MatDialogRef<CreateEditDayOffComponent>,
    private dayOffService: DayOffService
  ) { 
    super(injector);
    this.dayOff = data.item;
  }

  ngOnInit() {
    this.title = "Create Day Off";
    if(this.dayOff.id != null) {
      this.dateControl = new FormControl(new Date(this.dayOff.dayOff), [Validators.required]);
      this.title = "Edit Day Off";
    }
  }

  save(): void {
    var date = new Date(this.dateControl.value);
    if(isNaN(date.getTime())) {
      abp.message.error(this.l("Please enter a valid date!"));
      return;
    }
    this.dayOff.dayOff = new Date(moment(date).format("YYYY-MM-DD")).toISOString();
    this.dayOffService.save(this.dayOff).subscribe(res => {
      if(res) {
        this.notify.success(this.l("Create Day Off Successfully!"));
      }
      this.onNoClick();
    });
  }

  onNoClick(): void {
    this.diaLogRef.close();
  }

}
