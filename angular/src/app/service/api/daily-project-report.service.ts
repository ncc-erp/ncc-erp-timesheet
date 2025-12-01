import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";

export interface DailyProjectTimelogReportResponse {
  name: string;
  members: string[];
  totalTimelogLW: number;
  totalTimelogLM: number;
}

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
    branchIds: number[],
    minHours?: number,
    limit?: number,
    projectIds?: number[],
    isAllBranch?: boolean
  ): Observable<DailyProjectTimelogReportResponse[]> {
    let params = new HttpParams();

    if (isAllBranch) {
      params = params.set("IsAllBranch", isAllBranch.toString());
    }

    if (!isAllBranch && branchIds.length) {
      branchIds.forEach((id) => {
        params = params.append("BranchId", id.toString());
      });
    }

    if (projectIds && projectIds.length) {
      projectIds.forEach((id) => {
        params = params.append("ProjectIds", id.toString());
      });
    }

    if (minHours !== undefined) {
      params = params.set("MinHours", minHours.toString());
    }

    if (limit !== undefined) {
      params = params.set("Limit", limit.toString());
    }

    return this.http
      .get<any>(
        this.getUrl("GetDailyProjectTimelogReport"),
        { params }
      )
      .pipe(
        map(response => response.result ? response.result as DailyProjectTimelogReportResponse[] : response as DailyProjectTimelogReportResponse[])
      );
  }
}
