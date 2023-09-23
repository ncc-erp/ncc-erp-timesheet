

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';


@Injectable({
    providedIn: 'root'
  })
  export class TeamBuildingService extends BaseApiService{
    constructor(http: HttpClient) {
        super(http);
      }

    changeUrl() {
      return 'TeamBuildingProject';
    }

    getAll(request): Observable<any> {
      return this.http.post(this.rootUrl + "/GetAllPagging?", request);
    }

    changeIsTeamBuilding(input): Observable<any> {
        return this.http.put(this.rootUrl + '/ChangeIsTeamBuilding', input);
      }
  }
