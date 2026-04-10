import { Component, Inject, Injector, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { ManageUserProjectForBranchService } from '@app/service/api/manage-user-project-for-branch.service';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import * as moment from 'moment';
import { PopupCustomeTimeComponent } from '../detail-participating-projects/popup-custome-time/popup-custome-time.component';
import { ProjectHistoryDto } from '../../Dto/project-history-dto';
import { MatSelectChange } from '@angular/material/select';

@Component({
    selector: 'app-project-history',
    templateUrl: './project-history.component.html',
    styleUrls: ['./project-history.component.css']
})
export class ProjectHistoryComponent extends AppComponentBase implements OnInit {
    public viewChange = new FormControl(this.APP_CONSTANT.TypeViewHomePage.Year);
    private activeView: number = 0;
    public projectList: ProjectHistoryDto[] = [];
    public distanceFromAndToDate = '';
    public typeDate: any;
    public userId: number;
    public fromDate: any;
    public toDate: any;
    public viewOptions: any[] = [];
    public showInactiveProject: boolean = false;

    constructor(
        injector: Injector,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private _dialog: MatDialog,
        private _dialogRef: MatDialogRef<ProjectHistoryComponent>,
        private manageUserProjectForBranchService: ManageUserProjectForBranchService,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.userId = this.data.user.id;
        this.viewOptions = this.APP_CONFIG.TypeViewHomePage.filter(
            (item: any) => item.value !== this.APP_CONSTANT.TypeViewHomePage.Week
        );
        if (this.data.startDate == "" || this.data.endDate == "") {
            this.viewChange = new FormControl(this.APP_CONSTANT.TypeViewHomePage.AllTime);
            this.changeView(true);
        } else {
            this.changeView(false, moment(this.data.startDate), moment(this.data.endDate));
        }
    }

    closeDialog(): void {
        this._dialogRef.close();
    }

    getData(userId, fromDate, toDate) {
        this.manageUserProjectForBranchService
            .getUserProjectHistory(userId, fromDate, toDate)
            .subscribe(res => {
                this.projectList = res.result; 
            });
    }

    setFromAndToDate(fromDate, toDate) {
        this.fromDate = fromDate;
        this.toDate = toDate;
    }

    changeView(reset?: boolean, fDate?: any, tDate?: any) {
        if (reset) {
            this.activeView = 0;
        }
        let fromDate, toDate;
        if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Month) {
            fromDate = moment().startOf('M').add(this.activeView, 'M');
            toDate = moment(fromDate).endOf('M');
            this.typeDate = 'Month';
        }
        if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Quater) {
            fromDate = moment().startOf('Q').add(this.activeView, 'Q');
            toDate = moment(fromDate).endOf('Q');
            this.typeDate = 'Quarter';
        }
        if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Year) {
            fromDate = moment().startOf('y').add(this.activeView, 'y');
            toDate = moment(fromDate).endOf('y');
            this.typeDate = 'Year';
        }
        if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.AllTime) {
            fromDate = '';
            toDate = '';
            this.distanceFromAndToDate = 'All Time';
        }
        if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
            fromDate = '';
            toDate = '';
            if (!reset && fDate && tDate) {
                if (fDate && tDate) {
                    fromDate = fDate.format('DD MMM YYYY');
                    toDate = tDate.format('DD MMM YYYY');
                }
                this.setFromAndToDate(fromDate, toDate);
                this.getData(this.userId, fromDate, toDate);
                this.distanceFromAndToDate = fromDate + '  -  ' + toDate;
            } else {
                this.distanceFromAndToDate = 'Custom Time';
            }
        }

        if (fromDate != '' && toDate != '') {
            let fDateStr = '', tDateStr = '';
            let list = [];
            list[0] = { value: fromDate.isSame(toDate, 'year'), type: 'YYYY' };
            list[1] = { value: fromDate.isSame(toDate, 'month'), type: 'MMM' };
            list[2] = { value: fromDate.isSame(toDate, 'day'), type: 'DD' };
            list.map(value => {
                if (value.value) {
                    tDateStr = toDate.format(value.type) + ' ' + tDateStr;
                } else {
                    fDateStr = fromDate.format(value.type) + ' ' + fDateStr;
                    tDateStr = toDate.format(value.type) + ' ' + tDateStr;
                }
            });
            this.distanceFromAndToDate = this.typeDate + ': ' + fDateStr.trim() + ' - ' + tDateStr.trim();
        }
        
        if (this.viewChange.value != this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
            fromDate = fromDate == '' ? '' : fromDate.format('YYYY-MM-DD');
            toDate = toDate == '' ? '' : toDate.format('YYYY-MM-DD');
            this.getData(this.userId, fromDate, toDate);
            this.setFromAndToDate(fromDate, toDate);
        }
    }

    nextOrPre(title: any): void {
        if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
            return;
        }
        if (title == 'pre') {
            this.activeView--;
        }
        if (title == 'next') {
            this.activeView++;
        }
        this.changeView();
    }

    onSelectionChange(event: MatSelectChange) {
        if (event.value === this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
            this.showPopupCustomTime();
        }
        
        this.changeView(true);
    }

    showPopupCustomTime() {
        let popupCustomTime = this._dialog.open(PopupCustomeTimeComponent);
        popupCustomTime.afterClosed().subscribe(result => {
            if (result != undefined) {
                if (result.result) {
                    this.changeView(false, result.data.fromDateCustomTime, result.data.toDateCustomTime);
                }
            }
        });
    }

    getProjectUserType(userType: number) {
        let type = this.APP_CONSTANT.EnumUserType;
        let result = 'Unknown';
        for (let key in type) {
            if (type[key] == userType) {
                result = key;
                break;
        }
    }
    return result;
  }
}