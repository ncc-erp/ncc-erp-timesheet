import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { Component, OnInit } from '@angular/core';
import { GetPagingCapabilitySettingDto, ParamCapability, UserType } from '@app/service/api/model/capability-setting.dto';
import { DropdownData } from '../capabilities/capability.component';
import { Injector } from '@angular/core'
import { CapabilityType, CapabilityUserType, EComparisionOperator } from '@shared/AppEnums';
import { PositionDto } from '@app/service/api/model/position-dto';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { CapabilitySettingService } from '@app/service/api/capability-setting.service';
import { finalize } from 'rxjs/operators'
import { PositionService } from '@app/service/api/position.service';
import { ListFilterPipe } from '@shared/pipes/listFilter.pipe';
import { InitParam } from '../review/review-detail/review-detail.component';
import { Router } from '@angular/router';
@Component({
  selector: 'app-capability-setting',
  templateUrl: './capability-setting.component.html',
  styleUrls: ['./capability-setting.component.css']
})
export class CapabilitySettingComponent extends PagedListingComponentBase<GetPagingCapabilitySettingDto> implements OnInit {
  VIEW = PERMISSIONS_CONSTANT.ViewCapabilitySetting;
  ADD = PERMISSIONS_CONSTANT.AddNewCapabilitySetting;
  EDIT = PERMISSIONS_CONSTANT.EditCapabilitySetting;
  DELETE = PERMISSIONS_CONSTANT.DeleteCapabilitySetting;
  CLONE = PERMISSIONS_CONSTANT.CloneCapabilitySetting;
  
  public listPaging: GetPagingCapabilitySettingDto[] = []
  public listCapabilitySetting: GetPagingCapabilitySettingDto[] = []
  public userTypeList: UserType[] = []
  public userTypeDropDown: DropdownData[] = []
  public positionList: PositionDto[] = []
  public capabilityTypeDropdown: DropdownData[] = this.convertEnumToDropdown(CapabilityType)
  public positionDropDown: DropdownData[] = []
  public CAPABILITY_TYPE = CapabilityType
  public capabilityType: number = InitParam.ALL;
  public userType: number = InitParam.ALL;
  public positionId: number = InitParam.ALL;
  constructor(injector: Injector, 
    private _capabilitySettingService: CapabilitySettingService, 
    private _positionService: PositionService,
    private router: Router
    ) {
    super(injector)
  }
  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    request.searchText = this.searchText;
    request.filterItems = []
    if (this.userType != InitParam.ALL) {
      request.filterItems.push({
        comparison: EComparisionOperator.Equal,
        propertyName: 'userType',
        value: this.userType
      })
    }
    if (this.positionId != InitParam.ALL) {
      request.filterItems.push({
        comparison: EComparisionOperator.Equal,
        propertyName: 'positionId',
        value: this.positionId
      })
    }
    const param: ParamCapability = {
      param: request,
      type: this.capabilityType != InitParam.ALL ? this.capabilityType : null
    }
    this._capabilitySettingService.getAllPaging(param).pipe(finalize(() => finishedCallback())).subscribe(rs => {
      this.listPaging = rs.result.items;
      this.showPaging(rs.result, pageNumber)
    })
  }
  protected delete(entity: GetPagingCapabilitySettingDto): void {
    throw new Error('Method not implemented.');
  }
  ngOnInit() {
    this.getDataPage(1);
    this.getAllPosition();
    this.getAllUserType();
    this.capabilityTypeDropdown = [
      { key: 'All', value: InitParam.ALL },
      ...this.capabilityTypeDropdown]
    this.userTypeDropDown = [
      {
        key: 'All',
        value: InitParam.ALL
      },
      ...this.userTypeDropDown
    ]
  }
  getAllPosition() {
    this._positionService.getAll().subscribe(rs => {
      this.positionList = rs.result;
      this.positionDropDown = new ListFilterPipe().transform(this.positionList, "name", "id")
      this.positionDropDown = [
        { key: 'All', value: InitParam.ALL },
        ...this.positionDropDown]
    })
  }
  getAllUserType() {
    this._capabilitySettingService.getUserTypeForCapabilitySettings().subscribe(rs => {
      this.userTypeList = rs.result;
      this.userTypeDropDown = new ListFilterPipe().transform(rs.result, "name", "id")
      this.userTypeDropDown = [
        { key: 'All', value: InitParam.ALL },
        ...this.userTypeDropDown]
    })
  }
  onDelete(item: GetPagingCapabilitySettingDto){
    abp.message.confirm(`Remove all capabilities of ${item.userTypeName} ${item.positionName}`, (result) => {
      if(result) {
        this._capabilitySettingService.deleteGroupCapabilitySettings(item.userType, item.positionId).subscribe(rs => {
          abp.notify.success(`Removed all capabilities of ${item.userTypeName} ${item.positionName}`)
          this.refresh()
        })
      }
    })
  }
  onEdit(item: GetPagingCapabilitySettingDto) {
    this.router.navigate(["/app/main/capability-settings/capability-setting"], {
      queryParams: {
        userType: item.userType,
        positionId: item.positionId
      }
    })
  }
}