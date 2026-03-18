import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { AddWhitelistTypeDto, UpdateWhitelistTypeDto, WhitelistType } from './model/whitelist-system.dto';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Injectable({
    providedIn: 'root'
})
export class WhitelistSystemService extends BaseApiService {
    private whitelistTypes: WhitelistType[] = [];

    constructor(
        http: HttpClient
    ) {
        super(http);
    }

    changeUrl() {
        return 'WhitelistSystem';
    }

    getWhitelistTypes(): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetWhitelistTypes');
    }

    getAll(): Observable<any> {
        return this.http.get<any>(this.rootUrl + '/GetAll');
    }

    add(body: AddWhitelistTypeDto): Observable<any> {
        return this.http.post<any>(this.rootUrl + '/Add', body);
    }

    update(body: UpdateWhitelistTypeDto): Observable<any> {
        return this.http.put<any>(this.rootUrl + '/Update', body);
    }

    delete(id: number): Observable<any> {
        return this.http.delete<any>(this.rootUrl + '/Delete?id=' + id);
    }
}