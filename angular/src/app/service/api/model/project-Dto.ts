export interface GetProjectDto {
    id: number;
    name: string;
    code: string;
    status: number;
    customerName: string;
}

export interface ProjectDto {
    id: number;
    name: string;
    code: string;
    projectType: number;
    status: number;
    note: string;
    timeStart: Date;
    timeEnd: Date;
    customerId: number;
    isAllUserBelongTo: boolean;
    tasks: TaskProjectDto[];
    users: UserProjectDto[];
    projectTargetUsers: ProjectTargetUsersDto[];
    komuChannelId: string;
    isNotifyToKomu: boolean;
    isNoticeKMSubmitTS: boolean;
    isNoticeKMRequestOffDate: boolean;
    isNoticeKMApproveRequestOffDate: boolean;
    isNoticeKMRequestChangeWorkingTime: boolean;
    isNoticeKMApproveChangeWorkingTime: boolean;
}
export interface CustomerProjectDto {
    customerName: string;
    projects: any[];
}
export interface TaskProjectDto {
    // projectId: number,
    taskId: number,
    billable: true,
    id?: number,
    name?: string
}
export interface UserProjectDto {
    userId: number,
    // projectId: number,
    type: number,
    id?: number
    isTemp: boolean
}
export interface ProjectTargetUsersDto {
    userId: number,
    roleName: string,
    id?: number
}

export class BaseUserDto {
    name: string;
    isActive: boolean;
    type: number;
    jobTitle: string;
    level: number;
    avatarPath: string;
    avatarFullPath: string;
    branch: number;
    emailAddress: string;
    branchId: number;
    branchDisplayName: string;
    branchColor: string;
}

export class UserDto extends BaseUserDto {
    id: number;
}

export class AddUserToRoleDTO {
    userId: number;
    role: string;
}

export class ProjectUserDto extends BaseUserDto {
    userId: number;
    ptype: number;
    isTemp: boolean;
}

export class DisplayProjectTargetUserDto extends BaseUserDto {
    userId: number;
    roleName: string;
}

export class ProjectTaskDto {
    taskId: number;
    name: string;
    billable: boolean;
    type: number;
}

export class QuantityProjectDto {
    status : number;
    quantity : number;
}
