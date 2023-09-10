import { Observable } from 'rxjs';
import { BaseApiService } from '@app/service/api/base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ManageAbsenceTypesService extends BaseApiService{
  changeUrl() {
    return "AbsenceType";
  }

  constructor(http: HttpClient) { 
    super(http);
  }
  getAll() : Observable<any> {
    return this.http.get(this.rootUrl + "/GetAll");
  }
}
