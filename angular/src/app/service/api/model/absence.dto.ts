export class AbsenceRequestDto {
    id: number;
    userId: number;
    fullName: string;
    shortName: string;
    branch: number;
    type: number;
    avatarPath: string;
    avatarFullPath: string;
    level: number;
    sex: number;
    dateAt: string;
    dateType: number;
    hour: number;
    dayOffName: string;
    status: number;
    reason: string;
    name: string;
    leavedayType: number;
    absenceTime: number;
    createTime: string;
    createBy: string;
    lastModificationTime : string;
    lastModifierUserName : string;
    projectInfos : ProjectInfoDto[]
}

export class ProjectInfoDto {
    projectId : number;
    projectName : string;
    projectCode : string;
    pms : PmInfoDto[]
}

export class PmInfoDto {
    pmId : number;
    pmFullName : string;
    pmEmailAddress : string;
}
