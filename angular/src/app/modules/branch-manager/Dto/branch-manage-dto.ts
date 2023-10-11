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
}

export class projectListManagement {
    projectId: number;
    projectName: string;
    projectCode: string;
    status: number;
    valueOfUserType: number;
    shadowPercentage: number;
    workingHours: number;
}