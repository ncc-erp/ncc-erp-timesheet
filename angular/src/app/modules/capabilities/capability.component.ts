import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { Component, Injector, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material';
import { CapabilityService } from '@app/service/api/capability.service';
import { CapabilityDto } from '@app/service/api/model/capability.dto';
import { CapabilityType, ActionDialog, ExpressionEnum, EComparisionOperator } from '@shared/AppEnums';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { CreateCapabilityDialogData, CreateEditCapabilityDialogComponent } from './create-edit-capability-dialog/create-edit-capability-dialog.component';
import { finalize } from 'rxjs/operators'
import { toPlainObject } from 'lodash';
import { DomSanitizer } from '@angular/platform-browser';
@Component({
  selector: 'app-capability',
  templateUrl: './capability.component.html',
  styleUrls: ['./capability.component.css']
})
export class CapabilityComponent extends PagedListingComponentBase<CapabilityDto> implements OnInit {
  VIEW = PERMISSIONS_CONSTANT.ViewCapability;
  ADD = PERMISSIONS_CONSTANT.AddNewCapability;
  EDIT = PERMISSIONS_CONSTANT.EditCapability;
  DELETE = PERMISSIONS_CONSTANT.DeleteCapability;
  protected delete(entity: CapabilityDto): void {
    throw new Error('Method not implemented.');
  }
  public capabilityTypeList: DropdownData[] = []
  public capabilityList: CapabilityDto[] = []
  public filter = {
    capabilityType: DEFAULT_FILTER_VALUE
  }
  constructor(injector: Injector, private matDialog: MatDialog, private _capabilityService: CapabilityService, public sanitizer: DomSanitizer) {
    super(injector)
  }
  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    this.subscriptions.push(
      this._capabilityService.getAllPagging(request).pipe(finalize(() => finishedCallback())).subscribe(rs => {
        this.capabilityList = rs.result.items;
        this.showPaging(rs.result, pageNumber)
      })
    )
  }
  ngOnInit() {
    this.setDropdownData()
    this.refresh();
  }
  setDropdownData() {
    this.capabilityTypeList = Object.keys(CapabilityType)
      .filter(value => isNaN(Number(value)))
      .map(key => ({
        key: key,
        value: CapabilityType[key] as number
      }))
    this.capabilityTypeList = [ {
      key: 'All',
      value: DEFAULT_FILTER_VALUE
    },...this.capabilityTypeList]
  }
  onCreate(){
    const dialogRef = this.matDialog.open(CreateEditCapabilityDialogComponent, {
      data: {
        title: 'Create new capability',
        action: ActionDialog.CREATE,
      } as CreateCapabilityDialogData,
      panelClass: 'p-0',
      width:'750px'
    })
    dialogRef.afterClosed().subscribe(() => {
      this.refresh()
    })
  }
  onEdit(capability: CapabilityDto) {
    const dialogRef = this.matDialog.open(CreateEditCapabilityDialogComponent, {
      data: {
        title: 'Edit capability',
        action: ActionDialog.EDIT,
        capability: capability
      } as CreateCapabilityDialogData,
      panelClass: 'p-0',
      width: '750px'
    })
    dialogRef.afterClosed().subscribe(() => {
      this.refresh()
    })
  }
  onDelete(capability: CapabilityDto){
    abp.message.confirm(`Delete ${capability.name}`, (result) => {
      if(result) {
        this._capabilityService.delete(capability.id).subscribe(rs => {
          if(rs.success) {
            abp.notify.success("Capability deleted!")
            this.refresh()
          }
        })
      }
    })
  }
  getTypeName(type: number){
    const obj = toPlainObject(CapabilityType)
    return obj[type];
  }
  onCapabilityTypeChange(){
    if(this.filter.capabilityType !== DEFAULT_FILTER_VALUE) {
      this.filterItems = [
        {
          comparison: EComparisionOperator.Equal,
          value: this.filter.capabilityType,
          propertyName: 'type'
        }
      ]
    } else {
      this.filterItems = []
    }
    this.refresh()
  }
  getColor(userType: number){
    switch(userType){
      case this.UserType.Intern:
        return "#F57D00";
      case this.UserType.Staff:
        return "#014C6E";
      case this.UserType.Collaborator:
        return "#F9AE60";
      default:
        return "#000"
    }
  }
}
export interface DropdownData {
  key: string,
  value: number | string
}
export const DEFAULT_FILTER_VALUE = -1;