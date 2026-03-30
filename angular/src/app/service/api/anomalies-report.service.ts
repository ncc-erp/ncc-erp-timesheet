import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { BaseApiService } from "./base-api.service";
import { AbpResponse } from "@app/modules/branch-manager/Dto/branch-manage-dto";
import { AnomaliesTimelogReportResponse } from "@app/modules/branch-manager/Dto/anomalies-report-dto";

@Injectable({
  providedIn: "root",
})
export class AnomaliesReportService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "Report";
  }

  getAnomaliesTimelogReport(branchIds: number[]): Observable<AnomaliesTimelogReportResponse> {
    let params = new HttpParams();

    branchIds.forEach(id => {
      params = params.append("BranchIds", id.toString());
    });

    return this.http
      .get<AbpResponse<AnomaliesTimelogReportResponse>>(
        this.getUrl("GetAnomaliesTimelogReport"),
        { params }
      )
      .pipe(map(res => res.result));
  }
}
