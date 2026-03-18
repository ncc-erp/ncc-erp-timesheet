import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { AddUserWhitelistDto, UpdateUserWhitelistDto } from './model/user-whitelist.dto';

@Injectable({
    providedIn: 'root'
})
export class UserWhitelistService extends BaseApiService {

    constructor(
        http: HttpClient
    ) {
        super(http);
    }

    changeUrl() {
        return 'UserWhitelist';
    }

    getAll(): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetAll');
    }

    add(body: AddUserWhitelistDto): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/Add', body);
    }

    update(body: UpdateUserWhitelistDto): Observable<any> {
        return this.http.put<any>(this.rootUrl + '/Update', body); // Method PUT
    }

    delete(id: number): Observable<any> {
        return this.http.delete<any>(this.rootUrl + '/Delete?id=' + id);
    }

    downloadTemplate(): Observable<any> {
        return this.http.post(this.rootUrl + '/DownloadTemplate', null);
    }

    import(input: FormData): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/ImportFromExcel', input);
    }
}