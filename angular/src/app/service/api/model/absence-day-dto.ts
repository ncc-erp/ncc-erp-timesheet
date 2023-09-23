export class AbsenceRequestDto {
    userId: number;
    fullName: string;
    workStartHour: number;
    workStartMinute: number;
    dayOffName: string;
    status: number;
    reason: string;
    id: number;
    detail: AbsenceRequestDetailDto
    type: number;
}
export class AbsenceRequestDetailDto {
    dateAt: string;
    dateType: number;
    hour : number;
    id: number;
    absenceType: number;
    absenceTime:number
}
export class AbsenceDayRequest {
    userId: number;
    dayOffTypeId: number;
    status: number;
    reason: string;
    id: number;
    absences: AbsenceDayDto[];
    type: number;
}

 export class DayOffType {
    code: string;
    name: string;
    length: number;
    id: number;
}

 export class AbsenceDayDto {
    requestId: number;
    dateAt: string;
    hour: number;
    dateType: number;
    id: number;
    status: number;
    type: number;
    absenceTime: number;
}

export class LeaveDayDTO{
    titleLabel: string;
    statusLabel: string;
    absenceTypeLabel: string;
    reasonLabel: string;
    leaveDayLabel: string;
}

export class RequestOfUserDto {
    id: number;
    userId: number;
    branch: number;
    dateAt: string;
    dateType: number;
    hour: number;
    dayOffName: string;
    status: number;
    reason: string;
    leavedayType: number;
    isFuture: boolean;
    absenceTime: number;
}