export class ProjectHistoryDto {
    projectId: number;
    projectName: string;
    projectUserType: number;
    intervals: ProjectIntervalDto[];
    monthlyEfforts: MonthlyEffortDto[];
    status: number;
}

export class ProjectIntervalDto {
    startDate: string | Date;
    endDate: string | Date;
}

export class MonthlyEffortDto {
    monthYear: string;
    effort: number;
}