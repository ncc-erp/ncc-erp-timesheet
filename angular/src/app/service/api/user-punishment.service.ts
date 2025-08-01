import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { APP_CONSTANT } from '@app/constant/api.constants';

export interface UserPunishmentDto {
  id: number;
  dateAt: string;
  userId: number;
  userName: string;
  type: number;
  money: number;
  noteReply: string;
  punishmentSystemId: number;
  punishmentSystemName: string;
  totalMoney: number;
}

export interface CreateUserPunishmentDto {
  dateAt: string;
  userId: number;
  type: number;
  money: number;
  noteReply: string;
}

export interface UpdateUserPunishmentDto {
  id: number;
  dateAt: string;
  userId: number;
  type: number;
  money: number;
  noteReply: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserPunishmentService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'UserPunishment';
  }
  
  downloadTemplateImportUserPunishment(): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/DownloadTemplateImportUserPunishment', null);
  }
  
  importUserPunishmentFromFile(formData: FormData): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/ImportUserPunishmentFromFile', formData);
  }

  getAllUserPunishments(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetAll');
  }

  getUserPunishmentById(id: number): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get', {
      params: { id: id.toString() }
    });
  }

  create(userPunishment: CreateUserPunishmentDto): Observable<any> {
    return this.http.post(this.rootUrl + '/CreateUserPunishmentAsync', userPunishment);
  }

  update(userPunishment: UpdateUserPunishmentDto): Observable<any> {
    return this.http.put(this.rootUrl + '/UpdateUserPunishmentAsync', userPunishment);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/Delete', {
      params: { id: id.toString() }
    });
  }
}
