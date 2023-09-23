import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CustomerService extends BaseApiService{

  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl() {
    return 'Customer';
  }

  getAllCustomer(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetAll');
  }
  
}
 