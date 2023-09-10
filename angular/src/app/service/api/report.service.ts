import { Injectable } from '@angular/core';
import { HttpClient } from '@node_modules/@angular/common/http';
import { BaseApiService } from '@app/service/api/base-api.service';
import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { Observable } from '@node_modules/rxjs';

@Injectable({
    providedIn: 'root'
})
export class ReportService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl() {
        return 'NormalWorkingHour';
    }

    // @ts-ignore
    getAllPagging(request: PagedRequestDto, year: number, month: number, branchId: number, projectId: number,
        isThanDefaultWorking: boolean, checkInFilter: number, tsStatusFilter: number): Observable<any> {
        let url = `/GetAllPagging?year=${year}&month=${month}&branchId=${this.getPara(branchId)}&projectId=${this.getPara(projectId)}&isThanDefaultWorking=${isThanDefaultWorking}&checkInFilter=${this.getPara(checkInFilter)}&tsStatusFilter=${tsStatusFilter}`;
        return this.http.post<any>(this.rootUrl + url, request);
    }
    private getPara(value){
        if(value <=0) return '';
        return value
    }
    salaryNoice(request: PagedRequestDto, year: number, month: number): Observable<any> {
        return this.http.post<any>(this.rootUrl + "/SalaryNotice?year=" + year + '&month' + month, request)

    }
    conFirm(request: any, year: number, month: number): Observable<any> {
        return this.http.post<any>(this.rootUrl + "/Confirm?year=" + year + "&month=" + month, request)
    }

    ExportNormalWorking(request: PagedRequestDto, year: number, month: number, branchId: number, projectId: number,
        isThanDefaultWorking: boolean, checkInFilter: number, tsStatusFilter: number): Observable<any> {
        let url = `/ExportNormalWorking?year=${year}&month=${month}&branchId=${this.getPara(branchId)}&projectId=${this.getPara(projectId)}&isThanDefaultWorking=${isThanDefaultWorking}&checkInFilter=${this.getPara(checkInFilter)}&tsStatusFilter=${tsStatusFilter}`;
        return this.http.post<any>(this.rootUrl + url, request);
    }
    getNormalWorkingHourByUserLogin( year: number, month: number, status: number): Observable<any> {
        let url = `/GetNormalWorkingHourByUserLogin?year=${year}&month=${month}&status=${status}`;
        return this.http.get<any>(this.rootUrl + url);
    }
}
