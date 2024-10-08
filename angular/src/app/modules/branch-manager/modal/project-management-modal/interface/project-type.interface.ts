export interface  IProjectTargetUser {
    branchId: number;
    projectId: number;
    startDate: string;
    endDate: string;
    projectName: string;
    emailAddress: string;
    fullName: string;
    userType: number | string;
    workingPercent: string | number;
    totalWorkingTime: number;
    projectUserId: number;
}
