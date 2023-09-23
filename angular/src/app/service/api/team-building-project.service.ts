

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { SelectProjectIsAllowTeamBuildingDto } from '@app/modules/team-building/const/const';


@Injectable({
    providedIn: 'root'
  })
  export class TeamBuildingProjectService extends BaseApiService{
    constructor(http: HttpClient) {
        super(http);
      }

    changeUrl() {
      return 'TeamBuildingProject';
    }

    getAll(request): Observable<any> {
      return this.http.post(this.rootUrl + "/GetAllPagging?", request);
    }

    getAllProjectTeamBuilding(): Observable<any> {
      return this.http.get(this.rootUrl + "/GetAllProjectTeamBuilding");
    }

    selectIsAllowTeamBuilding(selectProjectIsAllowTeamBuildingDto: SelectProjectIsAllowTeamBuildingDto[]): Observable<any> {
      return this.http.post(this.rootUrl + "/SelectIsAllowTeamBuilding", selectProjectIsAllowTeamBuildingDto);
    }
  }
