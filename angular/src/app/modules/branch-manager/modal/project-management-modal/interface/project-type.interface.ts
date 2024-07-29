export interface  IProjectTargetUser {
    branchId: number;
    projectId: number;
    startDate: string;
    endDate: string;
    projectName: string;
    emailAddress: string;
    fullName: string;
    valueType: number | string;
    workingPercent: string | number;
}
