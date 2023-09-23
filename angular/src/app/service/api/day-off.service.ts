import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {BaseApiService} from './base-api.service';
import {Observable} from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class DayOffService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl() {
        return 'DayOff';
    }

    getAllDayOff(request, month?, year?): Observable<any> {
        let url = this.rootUrl + '/GetAllPagging?' + `year=${year}`;
        if (month) {
            url += `&month=${month}`;
        }
        return this.http.post(url, request);
    }

    deleteDayOff(id): Observable<any> {
        return this.http.delete(this.rootUrl + '/Delete?' + `Id=${id}`);
    }

    getAll(month, year, branchId): Observable<any> {
        return this.http.get(this.rootUrl + '/GetAll?' + `year=${year}&month=${month}&branchId=${branchId}`);
    }

    getAllDayOffType(): Observable<any> {
        return this.http.get(this.baseUrl + '/api/services/app/AbsenceType/GetAll');
    }
 }
