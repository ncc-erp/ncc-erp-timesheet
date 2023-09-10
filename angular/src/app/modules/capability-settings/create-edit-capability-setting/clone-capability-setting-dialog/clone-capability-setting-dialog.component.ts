import { Component, OnInit } from '@angular/core';
import { DropdownData } from '@app/modules/capabilities/capability.component';
import { AppComponentBase } from '@shared/app-component-base';
import { CapabilityUserType } from '@shared/AppEnums';
import { Injector, Inject } from '@angular/core'
import { PositionDto } from '@app/service/api/model/position-dto';
import { PositionService } from '@app/service/api/position.service';
import { ListFilterPipe } from '@shared/pipes/listFilter.pipe';
import { FormControl } from '@angular/forms'
import { cloneDeep } from 'lodash';
import { CloneCapabilitySettingDto } from '@app/service/api/model/capability-setting.dto';
import { CapabilitySettingService } from '@app/service/api/capability-setting.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { finalize } from 'rxjs/operators'
@Component({
  selector: 'app-clone-capability-setting-dialog',
  templateUrl: './clone-capability-setting-dialog.component.html',
  styleUrls: ['./clone-capability-setting-dialog.component.css']
})
export class CloneCapabilitySettingDialogComponent extends AppComponentBase implements OnInit {
  public saving: boolean = false;
  public title: string = ""
  public userTypeDropDown: DropdownData[]
  public positionList: PositionDto[] = []
  public positionDropdown: DropdownData[] = []
  public fromPositionDropDown: DropdownData[] = []
  public toPositionDropDown: DropdownData[] = []
  public fromUserType: number;
  public toUserType: number;
  public fromPositionId: number;
  public toPositionId: number;
  public searchFromPosition: FormControl = new FormControl("")
  public searchToPosition: FormControl = new FormControl("")
  public listFilterPipe: ListFilterPipe = new ListFilterPipe()
  constructor(
    injector: Injector, 
    private _positionService: PositionService, 
    private _capabilitySettingService: CapabilitySettingService, 
    @Inject(MAT_DIALOG_DATA) public data: CloneCapabilityDialogData,
    public dialogRef: MatDialogRef<CloneCapabilitySettingDialogComponent>
  ) { 
    super(injector)
  }
  getAllPosition() {
    this._positionService.getAll().subscribe(rs => {
      this.positionList = rs.result;
      this.positionDropdown = this.listFilterPipe.transform(this.positionList, "name", "id")
      this.fromPositionDropDown = cloneDeep(this.positionDropdown);
      this.toPositionDropDown = cloneDeep(this.positionDropdown)
    })
  }
  ngOnInit() {
    this.title = "Clone capabilities"
    if(this.data) {
      this.fromPositionId = this.data.fromPositionId;
      this.fromUserType = this.data.fromUserType
    }
    this.getAllPosition();
    this.searchFromPosition.valueChanges.subscribe(rs => this.fromPositionDropDown = this.filterPosition(rs))
    this.searchToPosition.valueChanges.subscribe(rs => this.toPositionDropDown = this.filterPosition(rs))
    this.userTypeDropDown = this.convertEnumToDropdown(CapabilityUserType).filter(x => (x.value == CapabilityUserType.Staff) || (x.value == CapabilityUserType.Internship))

  }
  filterPosition(value: string) {
    if(value) {
      const filteredList = this.positionList.filter(item => item.name.toLowerCase().includes(value.toLowerCase()) || item.shortName.toLowerCase().includes(value.toLowerCase()))
      const dropdownList = this.listFilterPipe.transform(filteredList, "name", "id")
      return dropdownList
    }
    else {
      const originalList = this.listFilterPipe.transform(this.positionList, "name", "id")
      return originalList
    }
  }
  public clone(close: boolean){
    const cloneCapabilitySettingDto: CloneCapabilitySettingDto = {
      fromPositionId: this.fromPositionId,
      fromUserType: this.fromUserType,
      toPositionId: this.toPositionId,
      toUserType: this.toUserType
    }
    this.saving = true;
    this._capabilitySettingService.cloneCapabilitySetting(cloneCapabilitySettingDto)
    .pipe(finalize(() => this.saving = false))
    .subscribe(rs => {
      if(rs.success) {
        abp.notify.success("Capability settings cloned!")
        if(close) {
          this.dialogRef.close()
        }
      }
    })
  }
  
  public close() {
    this.dialogRef.close()
  }
}
export interface CloneCapabilityDialogData {
  fromUserType: number,
  fromPositionId: number
}