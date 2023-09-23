import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { AppConsts } from '@shared/AppConsts';

@Injectable({
  providedIn: 'root'
})
export class CheckBoardService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return "Checkboard";
  }

  getAll(request: PagedRequestDto, month, year, code?): Observable<any> {
    if(code && code.trim().length != 0) {
      return this.http.post(this.rootUrl + '/GetAllPagging?' + `year=${year}&month=${month}&userCode=${code}`, request);
    } else return this.http.post(this.rootUrl + '/GetAllPagging?' + `year=${year}&month=${month}`, request);
  }

  confirmCheckBoard(param): Observable<any> {
    return this.http.post(this.rootUrl + "/ConfirmCheckBoards?" + `year=${param.year}&month=${param.month}`, param);
  }

  createCheckBoard(checkBoard): Observable<any> {
    return this.http.post(this.rootUrl + '/Create', checkBoard);
  }

  updateCheckBoard(checkBoard): Observable<any> {
    return this.http.post(this.rootUrl + "/Update", checkBoard);
  }

  deleteCheckBoard(checkboard): Observable<any> {
    var param = {id: checkboard.id};
    return this.http.post(this.rootUrl + "/Delete", param);
  }

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    const url = '/api/services/app/Checkboard/ImportCheckBoards';
    formData.append('file', file);
    const uploadReq = new HttpRequest('POST', AppConsts.remoteServiceBaseUrl + url,
      formData,
      {
        reportProgress: true
      }
    );
    return this.http.request(uploadReq);
  }
}
