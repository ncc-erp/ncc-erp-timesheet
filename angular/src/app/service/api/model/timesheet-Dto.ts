
export class TimeSheetDto {
    status: number;
    workingTime: number;
    dateAt: Date;
    projectId: number;
    user: string;
    userId: number;
    taskId: number;
    taskName: string;
    mytimesheetNote: string;
    customerName: string;
    projectName: string;
    projectCode: string;
    typeOfWork: number;
    isCharged: true;
    id: number;
    checked: boolean;
    isUserInProject: boolean;
    isUnlockedByEmployee? : boolean;
    avatarPath: string;
    avatarFullPath: string;
    branch:number;
    branchName: string;
    type: number;
    level: number;
    listPM: string[] = [];
    lastModificationTime: Date;
    branchColor: string;
    branchDisplayName: string;
    offHour : number;
    isOffDay: boolean;
}

export class TimeSheetGroupDto {
    groupName: string;
    selectedCount: number;
    totalCount: number;
    checked: boolean;
    timesheets: TimeSheetDto[] = []
}

export class ExportTimeSheetOrRemote {
    id: number;
    name: string;
    projectName: string;
    normalWorkingHours: number;
    overTime: number;
    dayOffType: number;
    timeCustom: number;
    absenceTime: number;
}
