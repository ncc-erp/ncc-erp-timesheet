import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";
import { PagedRequestDto } from "@shared/paged-listing-component-base";
import { AbpResponse, PagedResultDto, TotalTimelogProjectDto } from "@app/modules/branch-manager/Dto/branch-manage-dto";

@Injectable({
  providedIn: "root",
})
export class DailyProjectTimelogReportService extends BaseApiService{

  constructor(protected http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "Report";
  }

  getDailyProjectTimelogReport(
    request: PagedRequestDto,
    branchIds: number[],
    minHours?: number,
    projectIds?: number[],
    isAllBranch?: boolean,
    sortColumn?: string,
    sortDirection?: number
  ): Observable<AbpResponse<PagedResultDto<TotalTimelogProjectDto>>> {
    let params = new HttpParams();

    if (isAllBranch) {
      params = params.set("IsAllBranch", isAllBranch.toString());
    }

    if (!isAllBranch && branchIds.length) {
      branchIds.forEach((id) => {
        params = params.append("BranchIds", id.toString());
      });
    }

    if (projectIds && projectIds.length) {
      projectIds.forEach((id) => {
        params = params.append("ProjectIds", id.toString());
      });
    }

    if (minHours !== undefined && minHours !== null && minHours.toString().trim() !== '') {
      params = params.set("MinHours", minHours.toString());
    }

    if (sortColumn !== undefined) {
        params = params.set("Sort", sortColumn.toString());
    }

    if (sortDirection !== undefined) {
        params = params.set("SortDirection", sortDirection.toString());
    }

    params = params.set("SkipCount", request.skipCount.toString());
    params = params.set("MaxResultCount", request.maxResultCount.toString());

    if (request.searchText) {
      params = params.set("SearchText", request.searchText);
    }

    return this.http.get<AbpResponse<PagedResultDto<TotalTimelogProjectDto>>>(
        this.getUrl("GetDailyProjectTimelogReport"),
        { params }
    );
  }
}
