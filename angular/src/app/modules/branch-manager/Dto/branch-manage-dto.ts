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
