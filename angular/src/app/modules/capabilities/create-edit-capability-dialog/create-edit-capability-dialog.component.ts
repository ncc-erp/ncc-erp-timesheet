import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { CapabilityService } from '@app/service/api/capability.service';
import { CapabilityDto, CreateCapabilityDto } from '@app/service/api/model/capability.dto';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { AppComponentBase } from '@shared/app-component-base';
import { CapabilityType, ActionDialog } from '@shared/AppEnums';
import { cloneDeep, entries, isString, toPlainObject } from 'lodash';
import { finalize } from 'rxjs/operators';
@Component({
  selector: 'app-create-edit-capability-dialog',
  templateUrl: './create-edit-capability-dialog.component.html',
  styleUrls: ['./create-edit-capability-dialog.component.css']
})
export class CreateEditCapabilityDialogComponent extends AppComponentBase implements OnInit {
  public capabilityDto: CapabilityDto = {} as CapabilityDto
  public title: string = ""
  public saving: boolean = false
  public CapabilityTypeOptions = CapabilityType
  public dialogAction: ActionDialog = ActionDialog.CREATE;
  public readonly ACTIONDIALOG = ActionDialog
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
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: CreateCapabilityDialogData,
    injector: Injector,
    public dialogRef: MatDialogRef<CreateEditCapabilityDialogComponent>,
    private _capabilityService: CapabilityService)
    {
    super(injector)
   }
  ngOnInit() {
    if(this.data.action == ActionDialog.CREATE){
      this.title = "Create new capability";
      this.capabilityDto.type = CapabilityType.Point
    } else {
      this.title = "Edit capability ";
      this.capabilityDto = cloneDeep(this.data.capability)
    }
  }
  saveCapability(close: boolean){
    if(this.data.action == ActionDialog.CREATE){
      const payload: CreateCapabilityDto = {
        note: this.capabilityDto.note,
        name: this.capabilityDto.name,
        type: this.capabilityDto.type
      }
      this.saving = true;
      this.subscriptions.push(
        this._capabilityService.create(payload)
          .pipe(finalize(() => this.saving = false))
          .subscribe(rs => {
          if(rs.success) {
            abp.notify.success("New capability created");
            if(close) {
              this.dialogRef.close(true)
            }
          }
        })
      )
    }
    if(this.data.action == ActionDialog.EDIT) {
      const payload: CreateCapabilityDto = {
        note: this.capabilityDto.note,
        id: this.capabilityDto.id,
        name: this.capabilityDto.name,
        type: this.capabilityDto.type,
      }
      this.subscriptions.push(
        this._capabilityService.update(payload).subscribe(rs => {
          if(rs.success) {
            abp.notify.success("New capability created");
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
  getTypeName(type: number){
    const obj = toPlainObject(this.CapabilityTypeOptions)
    console.log(obj);
    return obj[type];
  }
}
export interface CreateCapabilityDialogData {
  capability?: CapabilityDto;
  action: ActionDialog
}