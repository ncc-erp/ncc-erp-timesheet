import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";
import { RetroDetailCreateEditDto } from "./model/retro-detail-dto";
import { InputGetUserOtherProjectRetroDto } from "@app/modules/retros/retro/retro-detail/app-add-user-other-project-retro-result/app-add-user-other-project-retro-result.component";
@Injectable({
  providedIn: "root",
})
export class RetroDetailService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return "RetroResult";
  }

  getAll(request, retroId): Observable<any> {
    return this.http.post(
      this.rootUrl + `/GetAllPagging?retroId=${retroId}`,
      request
    );
  }

  public getAllUsersNotInTheRetroResultByRetroId(id: number, projectId?: number): Observable<any> {
    if(id){
      return this.http.get(this.rootUrl + `/GetAllUsersNotInTheRetroResultByRetroId?retroId=${id}&projectId=${projectId}`);
    }
    return this.http.get(this.rootUrl + `/GetAllUsersNotInTheRetroResultByRetroId`);
  }
  public getAllUsers(retroId: number, projectId?: number): Observable<any> {
    return this.http.get(this.rootUrl + `/GetAllUsers?retroId=${retroId}${projectId?('&projectId='+projectId):''}`);
  }

  public getAllProject(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllProject");
  }
  public getProjectPMRetro(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetProjectPMRetro");
  }
  public getProjectPMRetroResult(retroId: number): Observable<any> {
    return this.http.get(this.rootUrl + "/GetProjectPMRetroResult?retroId=" + retroId);
  }

  public downloadTemplate(): Observable<any> {
    return this.http.post(this.rootUrl + "/DownloadTemplateImportRetro", null);
  }

  public ExportRetroResult(request, retroId): Observable<any> {
    return this.http.post(
      this.rootUrl + `/ExportRetroResult?retroId=${retroId}`,
      request
    );
  }

  public importUser(input: FormData): Observable<any> {
    return this.http.post(this.rootUrl + "/ImportRetroUserFromFile", input);
  }

  public ConfirmImportRetro(input: FormData): Observable<any> {
    return this.http.post(this.rootUrl + "/ConfirmImportRetro", input);
  }

  public getAllPms(retroId: number, projectId?: number): Observable<any> {
    return this.http.get(this.rootUrl + `/GetAllPms?retroId=${retroId}${projectId?('&projectId='+projectId):''}`);
  }

  public generateDataRetroResult(data: object): Observable<any> {
    return this.http.post(this.rootUrl + "/GenerateDataRetroResult", data);
  }

  public getAllRetroResultByPM(retroId: number, projectId?:number, branchId?: number): Observable<any> {
    let url = this.rootUrl + `/GetAllRetroResultByPM?retroId=${retroId}&`;
    let arr: string [] = [];

    if(projectId) {
      arr.push(`projectId=${projectId}`);
    }
    if(branchId) {
      arr.push(`branchId=${branchId}`);
    }
    if(arr.length > 0){
      url += arr.join('&');
    }

    return this.http.get(url);
  }

  public getRetroResultInfoUser(listEmpIds: number[], retroId: number): Observable<any> {
    return this.http.post(this.rootUrl + `/GetRetroResultInfoUser?retroId=${retroId}`, listEmpIds);
  }

  public getAllUser(input: InputGetUserOtherProjectRetroDto): Observable<any> {
    return this.http.post(this.rootUrl + "/GetUserNotPaggingOtherProjectRetroResult", input);
  }

  public addMultiUserRetroResult(input: RetroDetailCreateEditDto[]): Observable<any> {
    return this.http.post(this.rootUrl + "/AddMultiUserRetroResult", input);
  }
}
