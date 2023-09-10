import { PagedRequestDto } from 'shared/paged-listing-component-base';
import { Observable } from 'rxjs';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { BaseApiService } from '@app/service/api/base-api.service';
import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';

@Injectable({
  providedIn: 'root'
})
export class UserSalaryService extends BaseApiService {

  constructor(http : HttpClient) {
    super(http)
   }
   changeUrl() {
    return "UserSalary"
  }
  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    const url = '/api/services/app/UserSalary/ImportUserSalaryFromFile';
    formData.append('file', file);
    const uploadReq = new HttpRequest('POST', AppConsts.remoteServiceBaseUrl + url,
        formData,
        {
            reportProgress: true
        }
    );
    return this.http.request(uploadReq);
} 
  getAllPagging(page : PagedRequestDto):Observable<any> {
    return this.http.post(this.rootUrl + '/GetAllPagging',page)
  }
  delete(id : any) : Observable<any> {
    return this.http.delete(this.rootUrl + '/Delete?Id=' + id)
  }
  deactivateUserSalary(id: number): Observable<any> {
    return this.http.post(this.rootUrl + '/Deactive', {id: id});
}

activateUserSalary(id: number) {
    return this.http.post(this.rootUrl + '/Active', {id: id});
}
}
