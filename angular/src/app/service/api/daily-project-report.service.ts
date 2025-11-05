import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";

export interface DailyProjectTimelogReportResponse {
  reportDate: string;
  lastWeekStart: string;
  lastWeekEnd: string;
  lastMonth: string;
  projects: {
    name: string;
    members: string[];
    totalTimelogLW: number;
    totalTimelogLM: number;
  }[];
}

@Injectable({
  providedIn: "root",
})
export class DailyProjectTimelogReportService extends BaseApiService{

  constructor(protected http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "ProjectReport";
  }

  getDailyProjectTimelogReport(
    branchIds: number[],
    minHours?: number,
    topN?: number,
    projectIds?: number[]
  ): Observable<DailyProjectTimelogReportResponse> {
    let params = new HttpParams();

    if (branchIds.length) {
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

    if (topN !== undefined) {
      params = params.set("TopN", topN.toString());
    }

    return this.http
      .get<DailyProjectTimelogReportResponse>(
        this.getUrl("GetDailyProjectTimelogReport"),
        { params }
      )
      .pipe(
        map((response) => response)
      );
  }
}
