import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class OverTimeService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl() {
        return "OverTimeHour";
    }

    getAll(request, month, year, projectId): Observable<any> {
        return this.http.post(this.rootUrl + "/GetAllPagging?" + `year=${year}&month=${month}&projectId=${projectId < 1 ? '' : projectId}`, request);
    }
}