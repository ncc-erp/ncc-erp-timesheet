import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PagedRequestDto, PagedResultDto } from "@shared/paged-listing-component-base";
import { Observable } from "rxjs";
import { BaseApiService } from "./base-api.service";
import { ApiResponse } from "./model/api-response.model";
import { CapabilitySettingDto, CloneCapabilitySettingDto, CreateUpdateCapabilitySettingDto, ParamCapability, UserType } from "./model/capability-setting.dto";
@Injectable({
    providedIn: 'root'
})
export class    CapabilitySettingService extends BaseApiService {
    constructor(http: HttpClient) {
        super(http)
    }
    changeUrl() {
        return "CapabilitySetting"
    }
    getAllPaging(payload: ParamCapability): Observable<ApiResponse<PagedResultDto>> {
        return this.http.post<ApiResponse<PagedResultDto>>(this.rootUrl + "/GetAllPaging", payload)
    }
    getCapabilitiesByUserTypeAndPositionId(userType: number, positionId: number):Observable<ApiResponse<CapabilitySettingDto[]>> {
        return this.http.get<ApiResponse<CapabilitySettingDto[]>>(this.rootUrl + `/GetCapabilitiesByUserTypeAndPositionId?userType=${userType}&Positionid=${positionId}`)
    }
    getRemainCapabilitiesByUserTypeAndPositionId(userType: number, positionId: number):Observable<ApiResponse<CapabilitySettingDto[]>> {
        return this.http.get<ApiResponse<CapabilitySettingDto[]>>(this.rootUrl + `/GetRemainCapabilitiesByUserTypeAndPositionId?userType=${userType}&Positionid=${positionId}`)
    }
    createCapabilitySetting(payload: CreateUpdateCapabilitySettingDto):Observable<ApiResponse<CapabilitySettingDto>> {
        return this.http.post<ApiResponse<CapabilitySettingDto>>(this.rootUrl + `/CreateCapabilitySetting`, payload)
    }
    cloneCapabilitySetting(payload: CloneCapabilitySettingDto): Observable<ApiResponse<CapabilitySettingDto>> {
        return this.http.post<ApiResponse<CapabilitySettingDto>>(this.rootUrl + `/CapabilitySettingClone`, payload)
    }
    deleteCapabilitySetting(id: number):Observable<ApiResponse<void>>{
        return this.http.delete<ApiResponse<void>>(this.rootUrl + `/DeleteCapabilitySetting?id=${id}`)
    }
    updateCapabilitySetting(payload: CreateUpdateCapabilitySettingDto): Observable<ApiResponse<CapabilitySettingDto>> {
        return this.http.put<ApiResponse<CapabilitySettingDto>>(this.rootUrl + `/UpdateCapabilitySetting`, payload)
    }
    deleteGroupCapabilitySettings(userType: number, positionId: number): Observable<ApiResponse<void>> {
        return this.http.delete<ApiResponse<void>>(this.rootUrl + `/DeleteGroupCapabilitySettings?userType=${userType}&positionId=${positionId}`)
    }
    getUserTypeForCapabilitySettings(): Observable<ApiResponse<UserType[]>> {
        return this.http.get<ApiResponse<UserType[]>>(this.rootUrl + `/GetUserTypeForCapabilitySettings`)
    }
    deactiveCapabilitySettings(id: number, userType: number, positionId: number): Observable<ApiResponse<number>>{
        return this.http.get<ApiResponse<number>>(this.rootUrl +`/DeActive?id=${id}&usertype=${userType}&positionId=${positionId}`)
    }
}