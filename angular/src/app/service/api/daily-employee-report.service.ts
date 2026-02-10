import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { BaseApiService } from "./base-api.service";
import { Observable } from "rxjs";
import { PagedRequestDto } from "@shared/paged-listing-component-base";

@Injectable({
  providedIn: "root",
})
export class DailyEmployeeReportService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "Report";
  }

  getDailyProjectTimelogReport(
    request: PagedRequestDto,
    branchIds: number[],
    sortColumn?: number,
    sortDirection?: number,
    limit?: number
  ): Observable<any> {
    let params = new HttpParams();
    branchIds.forEach(id  => {
      params = params.append("BranchId", id.toString());
    });
    if (limit !== undefined) {
      params = params.set("Limit", limit.toString());
    }
    if (sortColumn !== undefined) {
      params = params.set("SortColumn", sortColumn.toString());
    }
    if (sortDirection !== undefined) {
      params = params.set("SortDirection", sortDirection.toString());
    }
    
    params = params.set("SkipCount", request.skipCount.toString());
    params = params.set("MaxResultCount", request.maxResultCount.toString());

    if (request.searchText) {
        params = params.set("SearchText", request.searchText);
    }

    return this.http.get<any>(
      this.getUrl("GetOfficeWorkingTimelogReport"),
      { params }
    );
  }

}
