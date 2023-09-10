import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { observable, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimesheetItemService extends BaseApiService{

  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl() {
    return 'MyTimesheets';
  }

}

