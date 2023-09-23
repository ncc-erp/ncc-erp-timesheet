import { Component, OnInit, Injector, Inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { CreateEditDayOffComponent } from '@app/modules/day-off/create-edit-day-off/create-edit-day-off.component';
import { DayOffService } from '@app/service/api/day-off.service';
import { AppComponentBase } from '@shared/app-component-base';
import { CalendarEvent } from 'angular-calendar';
import * as moment from 'moment';
import { dayOffDTO } from '../off-day.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-create-edit-off-day',
  templateUrl: './create-edit-off-day.component.html',
  styleUrls: ['./create-edit-off-day.component.css'],
  providers: [DatePipe]
})
export class CreateEditOffDayComponent extends AppComponentBase implements OnInit {

  title;
  coefficient = 2;
  dayOff: dayOffDTO = {} as dayOffDTO;
  event: CalendarEvent;
  dateShow: any = "";
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private diaLogRef: MatDialogRef<CreateEditDayOffComponent>,
    private dayOffService: DayOffService,
  ) {
    super(injector);
    this.event = data.item;
    this.title = "Create Day Off";
    this.dateShow = moment(this.data.date).format("DD-MM-YYYY");
    if (this.event.id) {
      this.title = "Edit Day Off";
      this.coefficient = +this.event.color.primary;
    }
  }

  ngOnInit() {
  }

  save(): void {
    if (isNaN(this.coefficient)) {
      abp.message.error(this.l("Coefficient must be a number!"));
      return;
    }
    if (+this.coefficient <= 0) {
      abp.message.error(this.l("Coefficient must be bigger than 0!"));
      return;
    }
    this.dayOff.dayOff = moment(this.data.date).format("YYYY-MM-DD");
    this.dayOff.name = this.event.title;
    this.dayOff.id = this.event.id == null ? 0 : +this.event.id;
    this.dayOff.coefficient = +this.coefficient;
    this.dayOffService.save(this.dayOff).subscribe(res => {
      if (res) {
        if (!res.result.id) {
          this.notify.success(this.l("Create Day Off Successfully!"));
        }
        else {
          this.notify.success(this.l("Update Day Off Successfully!"));
        }
      }
      this.diaLogRef.close();
    })
  }

  remove(): void {
    abp.message.confirm(this.l("Delete This Day Off?"), (result: boolean) => {
      if (result) {
        this.dayOffService.deleteDayOff(this.event.id).subscribe(res => {
          if (res) {
            this.notify.success(this.l("Delete Successfully!"));
          }
          this.diaLogRef.close();
        })
      }
    });
  }

}
