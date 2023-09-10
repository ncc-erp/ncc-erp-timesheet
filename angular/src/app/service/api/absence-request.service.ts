import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@node_modules/@angular/common/http";
import {BaseApiService} from "@app/service/api/base-api.service";
import {Observable} from "@node_modules/rxjs";

@Injectable({
    providedIn: 'root'
})
export class AbsenceRequestService extends BaseApiService {

    constructor(http: HttpClient) {
        super(http);
    }

    changeUrl() {
        return 'RequestDay';
    }

    getAll(startDate: string, endDate: string, status: number): Observable<any> {
        return this.http.get<any>(this.rootUrl + "/GetAll?startDate=" + startDate + "&endDate=" + endDate + "&status=" + status);
    }

    approveAbsenceRequest(reqId: any): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json-patch+json',
            })
        };
        return this.http.post<any>(this.rootUrl + '/ApproveRequest?requestId='+  reqId , httpOptions);
    }

    rejectAbsenceRequest(reqId: any): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json-patch+json',
            })
        };
        return this.http.post<any>(this.rootUrl + '/RejectRequest?requestId='+ reqId, httpOptions);
    }

    getAllRequestAbsence(sd, ed, id, name, offType, dayoffTypeId,dayAbsentStatus, dayType): Observable<any> {
        return this.http.post(this.rootUrl + "/GetAllRequest", {startDate: sd, endDate: ed, projectIds: id, name: name, type:offType, dayoffTypeId: dayoffTypeId,  status: dayAbsentStatus, dayType: dayType > 0 ? dayType : undefined});
    }

    getAllRequestAbsenceOfTeam(sd, ed, id, name, offType, dayoffTypeId,dayAbsentStatus, dayType): Observable<any> {
        return this.http.post(this.rootUrl + "/GetAllRequestForUser",{startDate: sd, endDate: ed, projectIds: id, name: name, type:offType, dayoffTypeId: dayoffTypeId,  status: dayAbsentStatus, dayType: dayType > 0 ? dayType : undefined});
    }

    getAllRequestByUserId(sd, ed, userId, absentDayType, dayType): Observable<any> {
        return this.http.get(this.rootUrl + `/GetAllRequestByUserId?startDate=${sd}&endDate=${ed}&userId=${userId}&type=${absentDayType}${dayType < 0 ? '' : `&dayType=${dayType}`}`);
    }

    getAllRequestByUserIdForTeamMember(sd, ed, userId, absentDayType, dayType ): Observable<any> {
        return this.http.get(this.rootUrl + `/GetAllRequestByUserIdForTeamMember?startDate=${sd}&endDate=${ed}&userId=${userId}&type=${absentDayType}${dayType < 0 ? '' : `&dayType=${dayType}`}`);
    }

    getUserRequestByDate(date, userId): Observable<any> {
        return this.http.get(this.rootUrl + "/GetUserRequestByDate?dateAt=" + date + "&userId=" + userId);
    }

    getAllRequestOfUserByDate(dateAt): Observable<any> {
        return this.http.get(this.rootUrl + "/GetAllRequestOfUserByDate?dateAt=" + dateAt);
    }
    cancelAbsenceRequest(reqId: any): Observable<any> {
        return this.http.delete(this.rootUrl + '/CancelRequest?' + `Id=${reqId}`);
    }

    notifyApproveRequestOffToPM(sd, ed, id, name, offType, dayoffTypeId,dayAbsentStatus, dayType): Observable<any> {
        return this.http.post(this.rootUrl + "/NotifyApproveRequestOffToPM",{startDate: sd, endDate: ed, projectIds: id, name: name, type:offType, dayoffTypeId: dayoffTypeId,  status: dayAbsentStatus, dayType: dayType > 0 ? dayType : undefined});
    }
    ExportTeamWorkingCalender(sd, ed, id, offType,dayAbsentStatus,branchId,dayOffTypeId1): Observable<any> {
        return this.http.post(this.rootUrl + "/ExportTeamWorkingCalender",{startDate: sd, endDate: ed, projectIds: id, type:offType, status: dayAbsentStatus,BranchId:branchId,dayOffTypeId:dayOffTypeId1});
    }
}
