import { BranchToDisplayDto } from "./branch-manage-dto";

export interface YesterdayAnomaly {
    userId: number;
    employeeName: string;
    userName: string;
    date: string | Date;
    actualHours: number;
    notes: string;
    branch: BranchToDisplayDto;
    isUnplannedAbsence: boolean;
}

export interface DateWithNote {
    date: string | Date;
    note: string;
}

export interface LastWeekAnomaly {
    userId: number;
    employeeName: string;
    userName: string;
    datesMissed: DateWithNote[];
    datesBelowThreshold: DateWithNote[];
    count: number;
    branch: BranchToDisplayDto;
}

export interface AnomaliesTimelogReportResponse {
    yesterdayAnomalies: YesterdayAnomaly[] | { items: YesterdayAnomaly[] };
    lastWeekAnomalies: LastWeekAnomaly[] | { items: LastWeekAnomaly[] };
}
