import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { AddNewUserToRemoteBlacklistDto, GetRemoteBlacklistDto, ImportRemoteBlacklistResultDto, UpdatePenaltyDaysDto } from './model/remote-blacklist.dto';

@Injectable({
  providedIn: 'root'
})
export class RemoteBlacklistService extends BaseApiService {
    constructor(
        http: HttpClient
    ) {
        super(http);
    }

    changeUrl() {
        return 'RemoteBlacklist';
    }

    addNewUser(item: AddNewUserToRemoteBlacklistDto): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/AddNewUser', item);
    }

    getMaxRemoteDays(): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetMaxRemoteDays');
    }

    getAll(request: any): Observable<any> {
        return this.http.get(this.rootUrl + '/GetAll', { params: request });
    }

    update(item: UpdatePenaltyDaysDto): Observable<any> {
        return this.http.put<any>(this.rootUrl + '/Update', item);
    }

    delete(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.rootUrl + '/Delete', {
            params: { id: id.toString() }
        });
    }

    downloadTemplate(): Observable<any> {
        return this.http.post(this.rootUrl + '/DownloadTemplate', null);
    }

    importFromExcel(input: FormData): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/ImportFromExcel', input);
    }
}