import { AppComponentBase } from '@shared/app-component-base';
import { OvertimeSettingService } from '../../../service/api/overtime-setting.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { OvertimeSettingDto } from '../overtime-setting.component';
import { FormControl } from '@angular/forms';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { GetProjectDto } from '@app/service/api/model/project-Dto';
import * as _ from 'lodash';
import * as moment from 'moment';
@Component({
  selector: 'app-create-edit-overtime-setting',
  templateUrl: './create-edit-overtime-setting.component.html',
  styleUrls: ['./create-edit-overtime-setting.component.css']
})
export class CreateEditOvertimeSettingComponent extends AppComponentBase implements OnInit  {
  overtimeSetting = {} as OvertimeSettingDto;
  title: string;
  active: boolean = true;
  respone = 0;
  saving: boolean = false;
  isSaving: boolean = false;

  listProject: GetProjectDto[] = [];
  projectSearch: FormControl = new FormControl("")
  listProjectFilter : GetProjectDto[];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    injector: Injector,
    private overtimeSettingService: OvertimeSettingService,
    private projectManageService: ProjectManagerService,
    private _dialogRef: MatDialogRef<CreateEditOvertimeSettingComponent>,
  ) {
    super(injector);
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });
   } 

  ngOnInit() {
    this.overtimeSetting = this.data;
    this.title = this.overtimeSetting.id != null ? 'Edit Overtime setting' : 'New Overtime setting';
    this.getListProject();
  }

  save() {
    if (isNaN(this.overtimeSetting.projectId)) {
      abp.message.error("Project is required!")
      return;
    }

    if (!this.overtimeSetting.dateAt) {
      abp.message.error("Date at is required!")
      return;
    }

    if (isNaN(this.overtimeSetting.coefficient)) {
      abp.message.error(this.l("Coefficient must be a number!"));
      return;
    }

    if(this.overtimeSetting.coefficient <= 0) {
      abp.message.error(this.l("Coefficient must be bigger than 0!"));
      return;
    }

    this.isSaving = true;
    this.overtimeSettingService.save(this.overtimeSetting).subscribe(res => {
      if (res.success == true) {
        if (this.overtimeSetting.id == null) {
          this.notify.success(this.l('Create Overtime setting successfully'));
        }
        else {
          this.notify.success(this.l('Update Overtime setting successfully'));
        }
        this.respone = 1;
        this.close(this.respone);
      }
    }, err => {
      this.isSaving = false;
    })
  }

  close(res): void {
    this._dialogRef.close(res);
  }

  getListProject() {
    this.projectManageService.getProjectFilter().subscribe(res => {
      this.listProject = res.result;
      this.listProjectFilter = this.listProject;
    });
  }

  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.listProject = this.listProjectFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.listProject = this.listProjectFilter.slice();
    }
  }
}
