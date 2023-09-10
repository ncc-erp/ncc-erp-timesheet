import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { CapabilitySettingService } from '@app/service/api/capability-setting.service';
import { CapabilitySettingDto, CreateUpdateCapabilitySettingDto, UserType } from '@app/service/api/model/capability-setting.dto';
import { PositionService } from '@app/service/api/position.service';
import { AppComponentBase } from '@shared/app-component-base';
import { DropdownData } from '../../capabilities/capability.component';
import { ActionDialog, CapabilityType, CapabilityUserType } from '@shared/AppEnums'
import { PositionDto } from '@app/service/api/model/position-dto';
import { ListFilterPipe } from '@shared/pipes/listFilter.pipe';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { cloneDeep, isNull } from 'lodash';
import { MatDialog, MatDialogActions, MatDialogRef } from '@angular/material';
import { CloneCapabilitySettingDialogComponent } from './clone-capability-setting-dialog/clone-capability-setting-dialog.component';
import { ActivatedRoute, Router } from '@angular/router';
import { UpdateCapabilitySettingComponent } from './update-capability-setting/update-capability-setting.component';
@Component({
  selector: 'app-create-edit-capability-setting',
  templateUrl: './create-edit-capability-setting.component.html',
  styleUrls: ['./create-edit-capability-setting.component.css'],
})
export class CreateEditCapabilitySettingComponent extends AppComponentBase implements OnInit {
  @ViewChild('selectedList') selectedList: CdkDragDrop<CapabilitySettingDto[]>
  @ViewChild('remainList') remainList: CdkDragDrop<CapabilitySettingDto[]>
  public userTypeList: UserType[] = []
  public userTypeDropDown: DropdownData[] = []
  public positionList: PositionDto[] = []
  public positionDropdown: DropdownData[] = []
  public capabilityList: CapabilitySettingDto[] = []
  public tempCapabilityList: CapabilitySettingDto[] = []
  public remainCapabilities: CapabilitySettingDto[] = []
  public tempRemainCapabilities: CapabilitySettingDto[] = []
  public searchCapability: string = ""
  public searchRemainCapability: string = ""
  public userType: number;
  public positionId: number;
  public CAPABILITY_TYPE = CapabilityType
  constructor(
    injector: Injector, 
    private _capabilitySettingService: CapabilitySettingService, 
    private _positionService: PositionService,
    private _dialog: MatDialog,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    ) {
    super(injector);
  }
  getRemainCapabilitiesForSetting(userType: number, positionId: number) {
    this._capabilitySettingService.getRemainCapabilitiesByUserTypeAndPositionId(userType, positionId).subscribe(rs => {
      this.remainCapabilities = rs.result
      if(!this.searchRemainCapability) {
        this.tempRemainCapabilities = rs.result
      }
    })
  }
  getCapabilitiesByUserTypeAndPosition(userType: number, positionId: number) {
    this._capabilitySettingService.getCapabilitiesByUserTypeAndPositionId(userType, positionId).subscribe(rs => {
      this.capabilityList = rs.result;
      if(!this.searchCapability) {
        this.tempCapabilityList = rs.result
      }
    })
  }
  getAllPosition() {
    this._positionService.getAll().subscribe(rs => {
      this.positionList = rs.result;
      this.positionDropdown = new ListFilterPipe().transform(this.positionList, "name", "id")
    })
  }
  getUserType(){
    this._capabilitySettingService.getUserTypeForCapabilitySettings().subscribe(rs => {
      this.userTypeList = rs.result;
      this.userTypeDropDown = new ListFilterPipe().transform(rs.result, "name", "id")
    })
  }
  ngOnInit() {
    this.refresh();
  }
  onUserTypeChange(userType: number) {
    if (this.positionId) {
      this.router.navigate([], {
        queryParams: {
          userType : this.userType,
          positionId: this.positionId
        }
      })
    }
  }
  onPositionChange(positionId: number) {
    if (!isNull(this.userType)) {
      if (this.positionId) {
        this.router.navigate([], {
          queryParams: {
            userType : this.userType,
            positionId: this.positionId
          }
        })
      }
    }
  }
  createNewCapabilitySettingForUserTypePosition(capabilitySetting: CapabilitySettingDto, callback) {
    const createCapabilitySettingDto: CreateUpdateCapabilitySettingDto = {
      capabilityId: capabilitySetting.capabilityId,
      coefficient: 1,
      id: 0,
      positionId: this.positionId,
      userType: this.userType,
      guildeLine : capabilitySetting.guildeLine,
    }
    this._capabilitySettingService.createCapabilitySetting(createCapabilitySettingDto).subscribe(rs => {
      if (rs.success) {
        abp.notify.success(`${capabilitySetting.capabilityName} added!`)
        this.getCapabilitiesByUserTypeAndPosition(this.userType, this.positionId)
        this.getRemainCapabilitiesForSetting(this.userType, this.positionId)
        callback(rs.result)
      }
    })
  }
  updateCapabilitySetting(capabilitySetting: CapabilitySettingDto) {
    const updateCapabilitySettingDto: CreateUpdateCapabilitySettingDto = {
      capabilityId: capabilitySetting.capabilityId,
      coefficient: capabilitySetting.coefficient,
      id: capabilitySetting.id,
      positionId: capabilitySetting.positionId,
      userType: capabilitySetting.userType,
      guildeLine: capabilitySetting.guildeLine,
    }
    this._capabilitySettingService.updateCapabilitySetting(updateCapabilitySettingDto).subscribe(rs => {
      if(rs.success) {
        abp.notify.success(`${capabilitySetting.capabilityName} coefficient updated`)
        this.capabilityList = cloneDeep(this.capabilityList.map(item => {
          if(item.id == capabilitySetting.id) {
            return rs.result
          } return item;
        }))
      }
    })
  }
  public onEditCapabilitySetting(item: CapabilitySettingDto) {
    const dialog = this._dialog.open(UpdateCapabilitySettingComponent, {
      data: { ...item,  action: ActionDialog.EDIT },
      width: '50vw',
      height:'69vh'
    })
    dialog.afterClosed().subscribe((rs) => {
      if (rs) {
        this.getCapabilitiesByUserTypeAndPosition(this.userType, this.positionId);
      }
      this.refresh();
    })
  }
  deleteCapabilitySettingForUserTypePosition(capabilitySetting: CapabilitySettingDto, callback: Function) {
    this._capabilitySettingService.deleteCapabilitySetting(capabilitySetting.id).subscribe(rs => {
      if (rs.success) {
        abp.notify.success(`${capabilitySetting.capabilityName} removed!`)
        this.getCapabilitiesByUserTypeAndPosition(this.userType, this.positionId)
        this.getRemainCapabilitiesForSetting(this.userType, this.positionId)
        callback(rs.result)
      }
    })
  }
  drop(event: CdkDragDrop<CapabilitySettingDto[]>) {
    if (event.previousContainer == event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex)
    } else {
      const item = event.previousContainer.data[event.previousIndex]
      if (event.previousContainer.id == "remainList") {
        this.createNewCapabilitySettingForUserTypePosition(item, (response: CapabilitySettingDto) => {
          transferArrayItem(
            event.previousContainer.data,
            event.container.data,
            event.previousIndex,
            event.currentIndex,
            );
            this.tempCapabilityList[event.currentIndex] = cloneDeep(response)
          })
      }
      else {
        this.deleteCapabilitySettingForUserTypePosition(item, (reponse: CapabilitySettingDto) => {
          transferArrayItem(
            event.previousContainer.data,
            event.container.data,
            event.previousIndex,
            event.currentIndex,
          );
        })
      }
    }
  }
  filterCapability(event){
    this.tempCapabilityList = this.capabilityList.filter(item => item.capabilityName.toLowerCase().includes(event.toLowerCase()))
  }
  filterRemainCapability(event){
    this.tempRemainCapabilities = this.remainCapabilities.filter(item => item.capabilityName.toLowerCase().includes(event.toLowerCase()))
  }
  onClone(){
    const cloneCapabilitySettingDialogComponent = this._dialog.open(
      CloneCapabilitySettingDialogComponent,
      {
        data: {
          fromUserType: this.userType,
          fromPositionId: this.positionId
        },
        width: '800px',
        panelClass: ''
      }
    )
    cloneCapabilitySettingDialogComponent.afterClosed().subscribe(() => {
      this.refresh();
    })
    
  }
  deactiveCapabilitySettings(id: number) {
    const capabilitySettingDto = {
      capabilityId: id,
      positionId: this.positionId,
      userType: this.userType,
    } as CapabilitySettingDto;
    this.createNewCapabilitySettingForUserTypePosition(capabilitySettingDto, null);
    // this._capabilitySettingService.deactiveCapabilitySettings(id, this.userType, this.positionId).subscribe(rs => {
    //   if (rs.success) {
    //     this.getCapabilitiesByUserTypeAndPosition(this.userType, this.positionId)
    //     this.getRemainCapabilitiesForSetting(this.userType, this.positionId)
    //   }
    // })
  }
  refresh() {
    this.getAllPosition();
    this.getUserType();
    this.activatedRoute.queryParams.subscribe(rs => {
      this.userType = Number(rs['userType']);
      this.positionId = Number(rs['positionId']);
      if(!isNull(this.userType) && !isNull(this.positionId) && !isNaN(this.userType) && !isNaN(this.positionId)) {
        this.getCapabilitiesByUserTypeAndPosition(this.userType, this.positionId)
        this.getRemainCapabilitiesForSetting(this.userType, this.positionId)
      }
    })
  }
}
