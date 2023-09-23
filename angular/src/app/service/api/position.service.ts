import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseApiService } from "./base-api.service";
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class PositionService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  GetAllPosition(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllPosition");
  }

  changeUrl() {
    return "Position";
  }
  getAll(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllPositionDropDownList");
  }
}
