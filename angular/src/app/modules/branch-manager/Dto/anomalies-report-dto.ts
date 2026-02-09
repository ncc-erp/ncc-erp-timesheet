export interface AnomalyBranch {
    branchName: string;
    branchColor: string;
}

export interface YesterdayAnomaly {
    userId: number;
    employeeName: string;
    userName: string;
    date: string | Date;
    actualHours: string;
    notes: string;
    branch: AnomalyBranch;
}

export interface LastWeekAnomaly {
    userId: number;
    employeeName: string;
    userName: string;
    datesMissed: (string | Date)[];
    datesNoTrackerTime: (string | Date)[];
    datesBelowThreshold: (string | Date)[];
    count: number;
    notes: string;
    branch: AnomalyBranch;
}

export interface AnomaliesTimelogReportResponse {
    yesterdayAnomalies: YesterdayAnomaly[] | { items: YesterdayAnomaly[] };
    lastWeekAnomalies: LastWeekAnomaly[] | { items: LastWeekAnomaly[] };
}
