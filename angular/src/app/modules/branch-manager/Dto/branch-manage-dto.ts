export class ManageUserDto {
    userName: string;
    emailAddress: string;
    fullName: string;
    projectUsers: ManageProjectUsersDto[];
    type: number;
    level: number;
    avatarPath: any;
    avatarFullPath: any;
    branch: string;
    branchDisplayName: string;
    position: string;
    positionName: string;
    projectCount: number;
    sex: number;
    id: number;
    hideProjectName: boolean;
}

export class ManageProjectUsersDto {
    projectId: number;
    projectCode: string;
    projectName: string;
    projectUserType: number;
    workingTimePercent: number;
    pms: string;
}

export class ProjectDto {
    projectId: number;
    projectCode: string;
    projectName: string;
    totalUser: number;
    deactiveCount: number;
    memberCount: number;
    shadowCount: number;
    pmCount: number;
}

export class ProjectTargetUserDto {
    fullName: string;
    emailAddress: string;
    workingTime: number | string;
    valueType: number | string;
}

export class ProjectListManagement {
    projectId: number;
    projectName: string;
    projectCode: string;
    status: number;
    valueOfUserType: number;    // abandoned
    projectUserType: number;
    effort: number;
    workingHours: number;
}

export interface UpdateTypeOfUsersInProjectDto {
    userTypes: UserTypeDto[];
    projectId: number;
}

export interface UserTypeDto {
    projectUserId: number;
    userType: number;
}

export interface OfficeWorkingItem {
    userId: number;
    fullName: string;
    userName: string;
    branchName: string;
    branchCode: string;
    branchColor: string;
    totalAllLW: number;
    officeLW: number;
    wfhLW: number;
    totalAllLM: number;
    officeLM: number;
    wfhLM: number;
    totalAllLWHours: number;
    officeLWHours: number;
    wfhLWHours: number;
    totalAllLMHours: number;
    officeLMHours: number;
    wfhLMHours: number;
}

export interface TotalTimelogProjectDto {
    name: string;
    members: string[];
    totalTimelogLW: number;
    totalTimelogLM: number;
}

export interface AbpResponse<T> {
    result: T;
    success: boolean;
    error: any;
    targetUrl: string;
    unAuthorizedRequest: boolean;
    __abp: boolean;
}

export interface PagedResultDto<T> {
    items: T[];
    totalCount: number;
}
