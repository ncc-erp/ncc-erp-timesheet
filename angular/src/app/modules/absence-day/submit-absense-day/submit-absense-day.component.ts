import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Inject, Injector } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
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
    @ViewChild('dateInput') dateInput: ElementRef;

    dayOffTypes = [] as DayOffType[];

    startDate: string;
    title: string;
    tempSelectedDate: any = null;

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
        if (this.absenceDayReq.absences.length !== 0) {
            this.absenceDayService.submitAbsenceDays(this.absenceDayReq).subscribe(resp => {
                if (resp && resp.success) {
                    const errorItem = resp.result.absences.find((item: any) => item.status == 3);
                    if (errorItem) {
                        const errorMsg = errorItem.errorMessage || 'Failed to submit absence request!';
                        this.notify.error(this.l(errorMsg));
                    } else {
                        this.data.selectedDays.clear();
                        this.notify.success(this.l('Submit absence days successfully!'));
                    }
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

    onCancel() {
        this.data.selectedDays.clear();
        this.absenceDayReq.absences = [];
        this.diaLogRef.close();
    }

    removeDate(index: number) {
        const removed = this.absenceDayReq.absences[index];
        this.absenceDayReq.absences.splice(index, 1);
        this.absenceDayReq.absences = [...this.absenceDayReq.absences];
        if (removed && removed.dateAt) {
            this.data.selectedDays.delete(removed.dateAt);
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

    addAnotherDate(event: any) {
        console.log('event', event);
        if (!event.value) return;
        const selectedDate = moment(event.value).format("YYYY-MM-DD");
        console.log('selectedDate', selectedDate);

        if (this.absenceDayReq.absences.findIndex(x => x.dateAt === selectedDate) >= 0) {
            this.notify.error('This date is already selected!');
            setTimeout(() => {
                this.tempSelectedDate = null;
                if (this.dateInput && this.dateInput.nativeElement) {
                    this.dateInput.nativeElement.value = '';
                }
            }, 0);
            return;
        }

        let dateType = 1;
        let hour = 0;
        let absenceTime = null;

        if (this.data.type === 0 && this.data.absenceTime === 1) {
            dateType = 4;
            absenceTime = 1;
        }

        const newAbsence = {
            dateAt: selectedDate,
            dateType: dateType,
            hour: hour,
            absenceTime: absenceTime,
            status: this.absenceDayReq.status
        } as AbsenceDayDto;

        this.absenceDayReq.absences = [...this.absenceDayReq.absences, newAbsence];

        this.data.selectedDays.set(selectedDate, this.data.type === 0 && this.data.absenceTime === 1 
            ? { type: dateType, hour: hour, absenceTime: absenceTime } 
            : dateType
        );

        setTimeout(() => {
            this.tempSelectedDate = null;
            if (this.dateInput && this.dateInput.nativeElement) {
                this.dateInput.nativeElement.value = '';
            }
        }, 0);
    }
}

