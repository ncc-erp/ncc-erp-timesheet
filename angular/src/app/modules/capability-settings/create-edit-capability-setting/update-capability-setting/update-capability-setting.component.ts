import { CapabilitySettingService } from '@app/service/api/capability-setting.service';
import { MatDialogRef } from '@angular/material/dialog';
import { Component, Inject, Injector, OnInit, Type } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';
import { CapabilitySettingDto, CreateUpdateCapabilitySettingDto } from '@app/service/api/model/capability-setting.dto';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { AppComponentBase } from '@shared/app-component-base';
import { CapabilityType, ActionDialog } from '@shared/AppEnums';
@Component({
  selector: 'app-update-capability-setting',
  templateUrl: './update-capability-setting.component.html',
  styleUrls: ['./update-capability-setting.component.css']
})
export class UpdateCapabilitySettingComponent extends AppComponentBase implements OnInit {
  constructor(injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data,
    public dialogRef: MatDialogRef<UpdateCapabilitySettingComponent>,
    public capabilitySettingService: CapabilitySettingService,
  ) {
    super(injector);
  }
  public capabilitySetting = {} as CapabilitySettingDto;
  public saving = false;
  public config: AngularEditorConfig = {
    editable: true,
    spellcheck: true,
    height: '30rem',
    minHeight: '8rem',
    placeholder: 'Enter text here...',
    translate: 'no',
    toolbarPosition: 'top',
    defaultFontName: 'Arial',
    customClasses: [
      {
        name: "quote",
        class: "quote",
      },
      {
        name: 'redText',
        class: 'redText'
      },
      {
        name: "titleText",
        class: "titleText",
        tag: "h1",
      },
    ],
    defaultParagraphSeparator: 'p',
    defaultFontSize: '4',
  };
  public CapabilityTypeEnum = CapabilityType
  ngOnInit() {
    this.capabilitySetting = this.data;
    console.log(this.data)
    
    
  }
  saveCapabilitySetting(close: boolean){
    if(this.data.action == ActionDialog.CREATE){
      const payload: CreateUpdateCapabilitySettingDto = {
        guildeLine: this.capabilitySetting.guildeLine,
        id: 0,
        coefficient: this.capabilitySetting.coefficient,
        userType: this.capabilitySetting.userType,
        positionId: this.capabilitySetting.positionId,
        capabilityId: this.capabilitySetting.capabilityId,
      }
      this.subscriptions.push(
        this.capabilitySettingService.createCapabilitySetting(payload).subscribe(rs => {
          if(rs.success) {
            abp.notify.success("New Capability Setting created!");
            if(close) {
              this.dialogRef.close(true)
            }
          }
        })
      )
    }
    if(this.data.action == ActionDialog.EDIT) {
      const payload: CreateUpdateCapabilitySettingDto = {
        guildeLine: this.capabilitySetting.guildeLine,
        id: this.capabilitySetting.id,
        coefficient: this.capabilitySetting.coefficient,
        userType: this.capabilitySetting.userType,
        positionId: this.capabilitySetting.positionId,
        capabilityId: this.capabilitySetting.capabilityId,
      }
      this.subscriptions.push(
        this.capabilitySettingService.updateCapabilitySetting(payload).subscribe(rs => {
          if(rs.success) {
            abp.notify.success("Capability Setting edited!");
            if(close) {
              this.dialogRef.close(true)
            }
          }
        })
      )
    }
  }
  public close(number){
    this.dialogRef.close();
  }
}
