import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UpdateUserInfoDto } from './model/my-profile-dto';

@Injectable({
  providedIn: 'root'
})
export class MyProfileService extends BaseApiService {
  changeUrl() {
    return "HRMv2";
  }

  constructor(http:  HttpClient) {
    super(http);
  }

  public getUserInfoByEmail(email: string) : Observable<any> {
    return this.http.get(this.rootUrl + `/GetUserInfoByEmail?email=`+email);
  }
  public getAllBanks(): Observable<any>{
    return this.http.get(this.rootUrl + "/GetAllBanks");
  }
  public getInfoToUpdate(email: string) : Observable<any> {
    return this.http.get(this.rootUrl + `/GetInfoToUpdate?email=`+email);
  }
  public updateUserInfo(input: UpdateUserInfoDto) : Observable<any> {
    return this.http.post(this.rootUrl + "/UpdateUserInfo", input);
  }

}
