import { HttpClient} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class KomuTrackerService extends BaseApiService {
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'KomuTracker';
  }

  // @ts-ignore
  getAllPagging(request: PagedRequestDto, dateAt: string, branchId: number, emailAddress: string): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/GetAllPagging?dateAt=' + dateAt + `&branchId=${branchId == 0 ? '' : branchId}`+ `&emailAddress=${emailAddress == '-1' ? '' : emailAddress}`, request);
  }
}
