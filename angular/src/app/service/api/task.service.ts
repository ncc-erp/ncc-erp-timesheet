import { BaseApiService } from './base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TaskService extends BaseApiService {
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'Task';
  }

  archiveTask(param: { id: number }): Observable<any> {
    return this.http.delete(this.rootUrl + '/Archive?Id=' + param);
  }
  unArchiveTask(id: number): Observable<any> {
    return this.http.post(this.rootUrl + '/DeArchive', {id});
  }

  getAll(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetAll');
  }

  deleteTask(param: { id: number }): Observable<any> {
    return this.http.delete(this.rootUrl + '/Delete?' + param)
  }

}
