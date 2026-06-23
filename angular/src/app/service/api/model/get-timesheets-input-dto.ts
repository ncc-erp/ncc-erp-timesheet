export interface GetTimesheetsInputDto {
    startDate: string;
    endDate: string;
    status?: number;
    projectId?: number;
    branchId?: number;
    checkInFilter?: number;
    searchText?: string;
    opentalkTime?: number;
    opentalkTimeType?: boolean;
    workLocation?: number;
    typeOfWork?: number;
    isCharged?: number;
    userId?: number;
}