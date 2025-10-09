import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

export interface OfficeWorkingReportSettingDto {
    enable: boolean;
    everyday: boolean;
    hour: number;
    officeIds: string;
    limit: number;
    mezonUrl: string;
}


@Injectable({
    providedIn: 'root'
})
export class OfficeWorkingReportSettingService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl(): string {
        return 'Configuration';
    }

    get(): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetOfficeWorkingReportSetting');
    }

    change(input: OfficeWorkingReportSettingDto): Observable<OfficeWorkingReportSettingDto> {
        return this.http.post<OfficeWorkingReportSettingDto>(this.rootUrl + '/SetOfficeWorkingReportSetting', input);
    }
}
