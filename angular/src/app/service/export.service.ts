import { DatePipe } from '@angular/common';
import { ExportTimeSheetOrRemote, TimeSheetDto } from './api/model/timesheet-Dto';
import { Injectable } from '@angular/core';
import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';
import { ReportTimeSheetDto, TardinessDto, WorkingReportDTO } from './api/model/report-timesheet-Dto';
import * as _ from 'lodash';
import * as moment from 'moment';


const EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
const EXCEL_EXTENSION = '.xlsx';

@Injectable({
    providedIn: 'root'
})

export class ExportService {
    Level = {
        Intern_0: 0,
        Intern_1: 1,
        Intern_2: 2,
        Intern_3: 3,
        FresherMinus: 4,
        Fresher: 5,
        FresherPlus: 6,
        JuniorMinus: 7,
        Junior: 8,
        JuniorPlus: 9,
        MiddleMinus: 10,
        Middle: 11,
        MiddlePlus: 12,
        SeniorMinus: 13,
        Senior: 14,
        SeniorPlus: 15,
    }

    userLevels = [
        { value: this.Level.Intern_0, name: "Intern_0", style: { 'background-color': '#B2BEB5' } },
        { value: this.Level.Intern_1, name: "Intern_1", style: { 'background-color': '#8F9779' } },
        { value: this.Level.Intern_2, name: "Intern_2", style: { 'background-color': '#665D1E' } },
        { value: this.Level.Intern_3, name: "Intern_3", style: { 'background-color': '#4B5320' } },
        { value: this.Level.FresherMinus, name: "Fresher-", style: { 'background-color': '#A1CAF1' } },
        { value: this.Level.Fresher, name: "Fresher", style: { 'background-color': '#89CFF0' } },
        { value: this.Level.FresherPlus, name: "Fresher+", style: { 'background-color': '#318CE7' } },
        { value: this.Level.JuniorMinus, name: "Junior-", style: { 'background-color': '#BFAFB2' } },
        { value: this.Level.Junior, name: "Junior", style: { 'background-color': '#A57164' } },
        { value: this.Level.JuniorPlus, name: "Junior+", style: { 'background-color': '#3B2F2F' } },
        { value: this.Level.MiddleMinus, name: "Middle-", style: { 'background-color': '#A4C639' } },
        { value: this.Level.Middle, name: "Middle", style: { 'background-color': '#8DB600' } },
        { value: this.Level.MiddlePlus, name: "Middle+", style: { 'background-color': '#008000' } },
        { value: this.Level.SeniorMinus, name: "Senior-", style: { 'background-color': '#F19CBB' } },
        { value: this.Level.Senior, name: "Senior", style: { 'background-color': '#AB274F' } },
        { value: this.Level.SeniorPlus, name: "Senior+", style: { 'background-color': '#E52B50' } },
    ];

    weekdays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']; 
    constructor() { }
    public exportTimeSheetProject(reportTimeSheets: ReportTimeSheetDto[], excelFileName: string): void {

        let workSheetUsers: WorkSheetUserDto[] = _(reportTimeSheets).groupBy(t => t.targetUserName)
            .map((value, key) => ({ name: key, worksheetItems: value }))
            .value();

        let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
        let totalUsers: TotalUser[] = [];
        workbook.SheetNames.push('Invoice');
        workSheetUsers.forEach((workSheetUser, index) => {
            const timesheets = workSheetUser.worksheetItems.map((s, i) => ({
                index: i + 1,
                dateAt: moment(s.dateAt).format('YYYY-MM-DD'),
                userName: s.targetUserName,
                roleName: s.roleName,
                typeOfWork: s.typeOfWork === 0 ? 'Normal Working' : 'OT',
                workingTimeNumber: !s.isShadow ? +((s.workingTime) / 60).toFixed(2) : +((s.targetUserWorkingTime) / 60).toFixed(2),
                workingTime: !s.isShadow ? +((s.workingTime) / 60 / 8).toFixed(2) : +((s.targetUserWorkingTime) / 60 / 8).toFixed(2),
                taskName: s.taskName,
                note: s.note,
            }))
            let sortTimeSheets = _.orderBy(timesheets, 'dateAt', 'asc');
            const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(sortTimeSheets);
            let totalNomarWorkingTime: number = 0;
            let totalOTWorkingTime: number = 0;
            let totalNormal: number = 0;
            let totalOT: number = 0;

            sortTimeSheets.forEach(user => {
                if (user.typeOfWork == 'OT') {
                    totalOTWorkingTime += user.workingTime;
                    totalOT += user.workingTimeNumber;
                }
                else {
                    totalNomarWorkingTime += user.workingTime;
                    totalNormal += user.workingTimeNumber;
                }
            })

            totalUsers.push({
                index: index + 1,
                name: workSheetUser.name,
                totalNormalNumber: totalNormal,
                totalNormal: +((totalNormal) / 8).toFixed(2),
                totalOTNumber: totalOT,
                totalOT: +((totalOT) / 8).toFixed(2)
            });
            workbook.SheetNames.push(workSheetUser.name);
            workbook.Sheets[workSheetUser.name] = worksheet;

            var wscols = [
                { wch: 5 },
                { wch: 15 },
                { wch: 15 },
                { wch: 20 },
                { wch: 15 },
                { wch: 10 },
                { wch: 10 },
                { wch: 20 },
                { wch: 30 },
            ];

            worksheet['!cols'] = wscols;

            var wsrows = [
                { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }
            ];
            worksheet['!rows'] = wsrows;
            // Header
            worksheet['A1'].v = '#';
            worksheet['B1'].v = 'Time Date';
            worksheet['C1'].v = 'User Name';
            worksheet['D1'].v = 'Role Name';
            worksheet['E1'].v = 'Type of Work';
            worksheet['F1'].v = 'Total Hours';
            worksheet['G1'].v = 'Man Day';
            worksheet['H1'].v = 'Task Description';
            worksheet['I1'].v = 'Note';

        })
        if (totalUsers && totalUsers.length > 0) {
            const worksheetInvoice: XLSX.WorkSheet = XLSX.utils.json_to_sheet(totalUsers);
            workbook.Sheets['Invoice'] = worksheetInvoice;
            var wscols2 = [
                { wch: 5 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 },
                { wch: 15 },
            ];
            worksheetInvoice['!cols'] = wscols2;
            worksheetInvoice['A1'].v = '#';
            worksheetInvoice['B1'].v = 'Name';
            worksheetInvoice['C1'].v = 'Total normal working hours';
            worksheetInvoice['D1'].v = 'Total normal working days';
            worksheetInvoice['E1'].v = 'Total OT hours';
            worksheetInvoice['F1'].v = 'Total OT days';
        }

        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, excelFileName);
    }

    public exportTimeSheet(timeSheet: TimeSheetDto[], fileName, type): void {
        let exportTS = _(timeSheet).groupBy(t => t.projectName).map((value, key) => ({ projectName: key, worksheetItems: value })).value();
        exportTS.forEach(e => {
            let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
            var name = fileName + e.projectName;
            let exportProjectTS = _(e.worksheetItems).groupBy(t => t.user).map((value, key) => ({ name: key, items: value })).value();
            exportProjectTS.forEach(data => {
                var temp = [], index = 1;
                data.items.reverse();
                data.items.forEach(d => {
                    temp.push({
                        index: index++,
                        date: moment(d.dateAt).format('YYYY-MM-DD'),
                        user: d.user,
                        type: d.typeOfWork == 0 ? 'Normal Working' : 'OT Working',
                        charge: d.isCharged ? "True" : "False",
                        time: +(d.workingTime / 60).toFixed(2),
                        day: +(d.workingTime / 60 / 8).toFixed(2),
                        task: d.taskName,
                        note: d.mytimesheetNote
                    });
                });
                if (type == "day") {
                    var dayTS = _(temp).groupBy(t => t.date).map((value, key) => ({ day: key, items: value })).value();
                    var listTS = [];
                    dayTS.forEach(dt => {
                        var hours = 0, note = "";
                        dt.items.forEach(t => {
                            hours += t.time;
                            note += t.note + "\n";
                        });
                        listTS.push({
                            index: listTS.length + 1,
                            time: dt.day,
                            user: dt.items[0].user,
                            hours: hours,
                            day: +(hours / 8).toFixed(2),
                            note: note
                        });
                    });
                    var wscols = [
                        { wch: 5 },
                        { wch: 15 },
                        { wch: 15 },
                        { wch: 10 },
                        { wch: 10 },
                        { wch: 30 },
                    ];
                    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(listTS);
                    workbook.SheetNames.push(data.name);
                    workbook.Sheets[data.name] = worksheet;
                    worksheet['!cols'] = wscols;
                    worksheet['A1'].v = '#';
                    worksheet['B1'].v = 'Time Date';
                    worksheet['C1'].v = 'User Name';
                    worksheet['D1'].v = 'Total Hours';
                    worksheet['E1'].v = 'Man Day';
                    worksheet['F1'].v = 'Note';

                    var wsrows = [
                        { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }
                    ];
                    worksheet['!rows'] = wsrows;
                } else {
                    var wscols = [
                        { wch: 5 },
                        { wch: 15 },
                        { wch: 15 },
                        { wch: 20 },
                        { wch: 15 },
                        { wch: 10 },
                        { wch: 10 },
                        { wch: 20 },
                        { wch: 30 },
                    ];
                    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(temp);
                    workbook.SheetNames.push(data.name);
                    workbook.Sheets[data.name] = worksheet;
                    worksheet['!cols'] = wscols;
                    worksheet['A1'].v = '#';
                    worksheet['B1'].v = 'Time Date';
                    worksheet['C1'].v = 'User Name';
                    worksheet['D1'].v = 'Type of Work';
                    worksheet['E1'].v = 'Charge';
                    worksheet['F1'].v = 'Total Hours';
                    worksheet['G1'].v = 'Man Day';
                    worksheet['H1'].v = 'Task Description';
                    worksheet['I1'].v = 'Note';

                    var wsrows = [
                        { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }, { hpx: 30 }
                    ];
                    worksheet['!rows'] = wsrows;
                }

            });
            const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
            this.saveAsExcelFile(excelBuffer, name);
        });
        // const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        // this.saveAsExcelFile(excelBuffer, fileName);
    }

    exportNormalHours(listReports: WorkingReportDTO[], dateOfMonth, fileName, type) {
        let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
        let listRowExcell = [];
        let index = 1;
        let UserType = {
            Staff: 0,
            Intern: 1,
            Collaborator: 2
        };
        listReports.map(x => {
            let res = {
                STT: index++,
                fullName: x.fullName,
                branch: x.branchDisplayName,
                type: x.type ? Object.keys(UserType)[x.type] : null,
                level: x.level ? this.userLevels[x.level].name : null,
                totalOpenTalk: x.totalOpenTalk,
                totalWorkingHour: x.totalWorkingHour == -1 ? "joined" : x.totalWorkingHour,
                totalWorkingday: x.totalWorkingday,
            };
            dateOfMonth.forEach(item => {
                res[item.date + " - " + item.day] = null;
            })
            x.listWorkingHour.forEach(item => {
                res[item.date + " - " + dateOfMonth.find(date => date.date == item.date).day] = item.workingHour;
            });
            listRowExcell.push(res);
        });
        const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(listRowExcell);
        worksheet['B1'].v = 'Full Name';
        worksheet['C1'].v = 'Branch';
        worksheet['D1'].v = 'User Type';
        worksheet['E1'].v = 'User Level';
        worksheet['F1'].v = 'Total Opentalk Hours';
        worksheet['G1'].v = 'Total Working Hour';
        worksheet['H1'].v = 'Total Working Day';
        workbook.SheetNames.push("Working Reports");
        workbook.Sheets["Working Reports"] = worksheet;
        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, fileName);
    }

    exportReportTardiness(listReports: TardinessDto[], fileName) {
        let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
        let listRowExcell = [];
        let index = 1;
        listReports.map(x => {
            let res = {
                STT: index++,
                UserName: x.userName,
                UserEmail: x.userEmail,
                NumberOfTardies: x.numberOfTardies,
                NumberOfLeaveEarly: x.numberOfLeaveEarly,
            };

            listRowExcell.push(res);
        });
        const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(listRowExcell);
        worksheet['B1'].v = 'Full Name';
        worksheet['C1'].v = 'Email';
        worksheet['D1'].v = 'Number of tardies';
        worksheet['E1'].v = 'Number of leave early';
        workbook.SheetNames.push("Report Tardiness");
        workbook.Sheets["Report Tardiness"] = worksheet;
        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, fileName);
    }

    exportLogTimeSheetDay(data: ExportTimeSheetOrRemote[], fileName){
        let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
        let listRowExcell = [];
        let index = 1;
        data.map(x=>{
            let res = {
                STT: index++,
                Name: x.name,
                ProjectName: x.projectName,
                normalWorkingHours: x.normalWorkingHours,
                overTime: x.overTime
            }
            listRowExcell.push(res);
        });
        var wscols = [
            { wch: 5 },
            { wch: 30 },
            { wch: 15 },
            { wch: 20 },
        ];
        const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(listRowExcell);
        worksheet['!cols'] = wscols;
        worksheet['B1'].v = 'Full Name';
        worksheet['C1'].v = 'Project Name';
        worksheet['D1'].v = 'Target User Working Time';
        worksheet['E1'].v = 'Over Time';
        workbook.SheetNames.push("Log TimeSheet Day");
        workbook.Sheets["Log TimeSheet Day"] = worksheet;
        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, fileName);
    }
    exportRemoteRequestDay(data: ExportTimeSheetOrRemote[], fileName){
        let workbook: XLSX.WorkBook = { Sheets: {}, SheetNames: [] };
        let listRowExcell = [];
        let index = 1;
        data.map(x=>{
            let type;
            if(x.dayOffType != 4) type = this.getAbsenceDayType(x.dayOffType);
            else {
                type = this.getTimeCustom(x.timeCustom)+ ': ' + x.absenceTime;
            }
            let res = {
                STT: index++,
                Name: x.name,
                Type: type
            }
            listRowExcell.push(res);
        });
        var wscols = [
            { wch: 5 },
            { wch: 30 },
            { wch: 15 },
        ];
        const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(listRowExcell);
        worksheet['!cols'] = wscols;
        worksheet['B1'].v = 'Full Name';
        worksheet['C1'].v = 'Absence Day Type';
        workbook.SheetNames.push("Request Remote Day");
        workbook.Sheets["Request Remote Day"] = worksheet;
        const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, fileName);
    }

    getBranch(x) {
        if (x.branch == 0) {
            return "Ha Noi";
        }
        if (x.branch == 1) {
            return "Da Nang";
        }
        if (x.branch == 2) {
            return "Ho Chi Minh";
        }
        return null;
    }
    getAbsenceDayType(x){
        if (x == 1) {
            return "Full Day";
        }
        if (x == 2) {
            return "Morning";
        }
        if (x == 3) {
            return "Afternoon";
        }
        if (x.branch == 4) {
            return "Custom";
        }
        return null;
    }
    getTimeCustom(x){
        if (x == 1) {
            return "Early hour";
        }
        if (x == 2) {
            return "Mid hour";
        }
        if (x == 3) {
            return "End hour";
        }
        return null;
    }
    private saveAsExcelFile(buffer: any, fileName: string): void {
        const data: Blob = new Blob([buffer], { type: EXCEL_TYPE });
        FileSaver.saveAs(data, fileName + EXCEL_EXTENSION);
    }
}


export class WorkSheetUserDto {
    name: string;
    worksheetItems: ReportTimeSheetDto[];
}

export class TotalUser {
    index: number;
    name: string;
    totalNormalNumber: number;
    totalNormal: number;
    totalOTNumber: number;
    totalOT: number;
}

export class TimeSheetByDay {
    index: number;
    time: string;
    user: string;
    hours: number;
    manDay: number;
    note: string;
}


