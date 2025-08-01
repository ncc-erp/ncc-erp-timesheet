import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { APP_CONSTANT } from '@app/constant/api.constants';

export interface PunishmentResponse {
  result: {
    totalCount: number;
    items: PunishmentDto[];
  };
  success: boolean;
  error: any;
  unAuthorizedRequest: boolean;
  __abp: boolean;
}

export interface PunishmentType {
  value: number;
  name: string;
}

export interface PunishmentDto {
  id: number;
  name: string;
  description: string;
  money: number;
  type: number;
  isActive: boolean;
  isDeleted: boolean;
  creationTime: string;
  lastModificationTime?: string;
  lastModifierUserId?: number;
  deleterUserId?: number;
  deletionTime?: string;
}

export interface FileBase64Dto {
  result?: {
    fileName: string;
    fileType: string;
    base64: string;
  };
  fileName?: string;
  fileType?: string;
  base64?: string;
  success?: boolean;
  targetUrl?: string;
  error?: any;
  unAuthorizedRequest?: boolean;
  __abp?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PunishmentService extends BaseApiService {
  private _punishmentTypes: PunishmentType[] = [];

  constructor(http: HttpClient) {
    super(http);
    this._punishmentTypes = APP_CONSTANT.PUNISHMENT_TYPES;
  }

  changeUrl() {
    return 'PunishmentSystem';
  }

  getAllPunishments(): Observable<PunishmentResponse> {
    return this.http.get<PunishmentResponse>(this.rootUrl + '/GetAllActivePunishmentSystemsAsync');
  }

  create(punishment: PunishmentDto): Observable<any> {
    return this.http.post(this.rootUrl + '/CreatePunishmentSystemAsync', punishment);
  }

  update(punishment: PunishmentDto): Observable<any> {
    return this.http.put(this.rootUrl + '/UpdatePunishmentSystemAsync', punishment);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/DeletePunishmentSystemAsync', {
      params: { id: id.toString() }
    });
  }

  deactivate(punishment: PunishmentDto): Observable<any> {
    const updatedPunishment = { ...punishment, isActive: false };
    return this.update(updatedPunishment);
  }

  activate(punishment: PunishmentDto): Observable<any> {
    const updatedPunishment = { ...punishment, isActive: true };
    return this.update(updatedPunishment);
  }

  getPunishmentTypes(): PunishmentType[] {
    return [...this._punishmentTypes];
  }
  
  downloadTemplateImportPunishment(): Observable<FileBase64Dto> {
    return this.http.post<FileBase64Dto>(this.rootUrl + '/DownloadTemplateImportPunishment', {});
  }

  importPunishmentFromFile(formData: FormData): Observable<any> {
    return this.http.post(this.rootUrl + '/ImportPunishmentFromFile', formData);
  }
}