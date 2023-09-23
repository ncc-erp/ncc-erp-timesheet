export class ApiResponse<T>{
    result?: T;
    success: boolean;
    error?: any;
    targetUrl?: string;
    unAuthorizedRequest: boolean;
}