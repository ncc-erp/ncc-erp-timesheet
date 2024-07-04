export interface  IProjectTargetUser {
    projectId: number;
    startDate: string;
    endDate: string;
    projectName: string;
    emailAddress: string;
    fullName: string;
    valueType: number | string;
    workingTime: string | number;
}
