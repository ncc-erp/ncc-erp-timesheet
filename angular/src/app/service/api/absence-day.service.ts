import {Injectable} from '@angular/core';
import {BaseApiService} from '@app/service/api/base-api.service';
import {HttpClient} from '@node_modules/@angular/common/http';
import {Observable} from '@node_modules/rxjs';
import {start} from "repl";

@Injectable({
    providedIn: 'root'
})
export class AbsenceDayService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl() {
        return 'RequestDay';
    }

    submitAbsenceDays(reqBody: any): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/SubmitToPendingNew', reqBody);
    }

    getAllAbsenceDays(year: number, month: number): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetAll?year=' + year + '&month=' + month);
    }

    getAllAbsenceReqs(sd, ed, absentDayType, dayType ): Observable<any> {
        return this.http.get(this.rootUrl + `/GetAllMyRequest?startDate=${sd}&endDate=${ed}&type=${absentDayType}${dayType < 0 ? '' : `&dayType=${dayType}`}`);
    }
    cancelAbsenceDayRequest(reqId: any):Observable<any> {
        return this.http.post(this.rootUrl + '/CancelMyRequest?requestId=' +reqId,{});
    }
}
