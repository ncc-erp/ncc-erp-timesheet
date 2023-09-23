import { Component, OnInit } from '@angular/core';
import { Inject, Injector } from '@node_modules/@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@node_modules/@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { AbsenceDayService } from '@app/service/api/absence-day.service';
import { DayOffService } from '@app/service/api/day-off.service';
import * as moment from 'moment';
import { AbsenceDayDto, DayOffType, AbsenceDayRequest } from '@app/service/api/model/absence-day-dto';
@Component({
    selector: 'app-submit-absense-day',
    templateUrl: './submit-absense-day.component.html',
    styleUrls: ['./submit-absense-day.component.css']
})
export class SubmitAbsenseDayComponent extends AppComponentBase implements OnInit {

    absenceDayReq: AbsenceDayRequest;

    dayOffTypes = [] as DayOffType[];

    startDate: string;
    title: string;

    endDate: string;
    isLoading: boolean
    isSaving: boolean = false;
    constructor(injector: Injector,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private absenceDayService: AbsenceDayService,
        private dayOffService: DayOffService,
        private diaLogRef: MatDialogRef<SubmitAbsenseDayComponent>) {
        super(injector);
    }

    ngOnInit() {
        if(this.data.type === 1){
            this.title = "Request onsite";
        } else if(this.data.type === 0 && this.data.absenceTime === 0) {
            this.title = "Request off";
        } else if(this.data.type === 2) {
            this.title = "Request remote";
        }
        else if(this.data.type === 0 && this.data.absenceTime === 1) {
            this.title = "Request đi muộn/về sớm";
        }
        this.absenceDayReq = new AbsenceDayRequest();
        this.absenceDayReq.reason = '';
        this.absenceDayReq.absences = [] as AbsenceDayDto[];
        if(this.data.type === 0 && this.data.absenceTime === 1){
            this.data.selectedDays.forEach((value, key) => {
                if (value.type !== undefined) {            
                    this.absenceDayReq.absences.push({
                        dateAt: moment(key).format("YYYY-MM-DD"),
                        dateType: value.type,
                        hour: value.hour,
                        absenceTime: value.absenceTime
                    } as AbsenceDayDto);
                }
            });
        }
        else{
            this.data.selectedDays.forEach((value, key) => {
                if (value.type === undefined) {
                    this.absenceDayReq.absences.push({
                        dateAt: moment(key).format("YYYY-MM-DD"),
                        dateType: value,
                        hour: 0,
                        status: this.absenceDayReq.status,
                        absenceTime: null
                    } as AbsenceDayDto);               
                }
            });
        }
        this.dayOffService.getAllDayOffType().subscribe(resp => {
            this.dayOffTypes = resp.result as DayOffType[];
            this.absenceDayReq.dayOffTypeId = this.dayOffTypes[0].id;
        });

        this.absenceDayReq.type = this.data.type;
    }

    submitReq() {
        if(this.data.type != this.APP_CONSTANT.DayAbsenceType.Remote && !this.absenceDayReq.reason.trim())
        {
            abp.message.error('Reason is require');
            return;
        }
        this.isLoading = true
        this.isSaving = true
        if (this.data.length !== 0) {
            this.absenceDayService.submitAbsenceDays(this.absenceDayReq).subscribe(resp => {
                if (resp) {
                    this.data.selectedDays.clear();
                    this.notify.success(this.l('Submit absence days successfully!'));
                }
                this.diaLogRef.close(true);
                this.isSaving = false
                this.isLoading = false
            }, (err) => {
                this.isSaving = false
                this.isLoading = false
            });
        }
       
    }
    getNameByValue(data: any) {
        if (data.value === 1) {
            return 'Full Day';
        }

        if (data.value === 2) {
            return 'Morning';
        }

        if (data.value === 3) {
            return 'Afternoon';
        }

        else {  
            if (data.value.absenceTime === this.APP_CONSTANT.OnDayType.EndOfDay) {
                return 'Về sớm';
            }
    
            if (data.value.absenceTime === this.APP_CONSTANT.OnDayType.BeginOfDay) {
                return 'Đi muộn';
            }
            return 'Off'; 
        
        }
    }
    getLissClass(data : any) {
        if (data.value == 1) {
            return 'day-chip-full-day';
        }
        if (data.value == 2) {
            return 'day-chip-morning';
        }
        if(data.value == 3) {
            return 'day-chip-afternoon';
        } 
        return 'day-chip-custom';
    }
}

