import { HttpClient } from "@angular/common/http";
import { PagedRequestDto, PagedResultDto } from "@shared/paged-listing-component-base";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";
import { ApiResponse } from "./model/api-response.model";
import { CapabilityDto, CreateCapabilityDto } from "./model/capability.dto";
import { Injectable } from "@angular/core";
@Injectable({
    providedIn: 'root'
})
export class CapabilityService extends BaseApiService {
    constructor(http: HttpClient){
        super(http)
    }
    changeUrl() {
        return 'Capability'
    }
    getAllPagging(request: PagedRequestDto): Observable<ApiResponse<PagedResultDto>> {
        return this.http.post<ApiResponse<PagedResultDto>>(this.rootUrl + "/GetAllPaging", request)
    }
    
    getAllNotPagging(): Observable<ApiResponse<CapabilityDto[]>> {
        return this.http.get<ApiResponse<CapabilityDto[]>>(this.rootUrl + "/GetAll")
    }
    public create(payload: CreateCapabilityDto): Observable<ApiResponse<CapabilityDto>> {
        return this.http.post<ApiResponse<CapabilityDto>>(this.rootUrl + "/create", payload)
    }
}