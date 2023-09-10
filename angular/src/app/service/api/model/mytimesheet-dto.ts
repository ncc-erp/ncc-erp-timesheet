export class WarningMyTimesheetDto {
    userId: number;
    dateAt: string;
    dateType: number;
    hourOff: number;
    type: number;
    absenceTime: number;
    workingTime: number;
    workingTimeLogged: number;
    checkIn: string;
    checkOut: string;
    checkInShow: string;
    checkOutShow: string;
    isWarning: boolean;
    hourDiMuon: number;
    hourVeSom: number;
    isOffDay: boolean;
}